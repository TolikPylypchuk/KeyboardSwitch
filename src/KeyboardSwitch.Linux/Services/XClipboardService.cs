using System.Reactive.Concurrency;
using System.Text;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class XClipboardService : ClipboardServiceBase
{
    private readonly X11Service x11;
    private readonly ILogger<XClipboardService> logger;

    private readonly IntPtr windowHandle;
    private readonly Atom[] atoms;
    private readonly Atom[] textAtoms;

    private string? storedText;

    private TaskCompletionSource<bool>? storeAtomSource;
    private TaskCompletionSource<Atom[]?>? requestedFormatsSource;
    private TaskCompletionSource<object?>? requestedDataSource;

    public XClipboardService(X11Service x11, IScheduler scheduler, ILogger<XClipboardService> logger)
        : base(scheduler)
    {
        this.x11 = x11;
        this.logger = logger;

        this.windowHandle = this.CreateEventWindow();
        this.atoms = new[]
        {
            this.x11.TargetsAtom,
            this.x11.MultipleAtom,
            Atom.String,
            this.x11.OemTextAtom,
            this.x11.Utf8StringAtom,
            this.x11.Utf16StringAtom
        }.Where(atom => atom != Atom.None).ToArray();

        this.textAtoms = new[] { this.x11.Utf16StringAtom, this.x11.Utf8StringAtom, this.x11.OemTextAtom, Atom.String }
            .Where(f => f != Atom.None)
            .ToArray();
    }

    public override async Task<string?> GetText()
    {
        this.LogGettingTextFromClipboard();

        if (XLib.XGetSelectionOwner(this.x11.Display, this.x11.ClipboardAtom) == IntPtr.Zero)
        {
            this.LogClipbaordSelectionOwnerAbsent();
            return null;
        }

        var response = await this.SendFormatRequest();

        var target = response is not null
            ? textAtoms.FirstOrDefault(f => response.Contains(f))
            : Atom.None;

        this.LogGettingText((ulong)target);

        var data = await this.SendDataRequest(target != Atom.None ? target : this.x11.Utf8StringAtom);
        return data?.ToString();
    }

    public override Task SetText(string text)
    {
        this.LogSettingTextIntoClipboard();

        this.storedText = text;
        XLib.XSetSelectionOwner(this.x11.Display, this.x11.ClipboardAtom, this.windowHandle, IntPtr.Zero);

        return this.StoreAtomsInClipboardManager();
    }

    private IntPtr CreateEventWindow()
    {
        var win = XLib.XCreateSimpleWindow(
            this.x11.Display,
            XLib.XDefaultRootWindow(this.x11.Display),
            0,
            0,
            1,
            1,
            0,
            IntPtr.Zero,
            IntPtr.Zero);

        this.x11.AddEventHandler(win, this.OnEvent);

        return win;
    }

    private Task<Atom[]?> SendFormatRequest()
    {
        if (this.requestedFormatsSource is null || this.requestedFormatsSource.Task.IsCompleted)
        {
            this.requestedFormatsSource = new();
        }

        this.LogSendingForwardRequestToX11();

        XLib.XConvertSelection(
            this.x11.Display,
            this.x11.ClipboardAtom,
            this.x11.TargetsAtom,
            this.x11.TargetsAtom,
            this.windowHandle,
            IntPtr.Zero);

        return this.requestedFormatsSource.Task;
    }

    private Task<object?> SendDataRequest(Atom format)
    {
        if (this.requestedDataSource is null || this.requestedDataSource.Task.IsCompleted)
        {
            this.requestedDataSource = new();
        }

        this.LogSendingDataRequestToX11();

        XLib.XConvertSelection(
            this.x11.Display, this.x11.ClipboardAtom, format, format, this.windowHandle, IntPtr.Zero);

        return this.requestedDataSource.Task;
    }

    private Task StoreAtomsInClipboardManager()
    {
        if (this.x11.ClipboardManagerAtom == Atom.None || this.x11.SaveTargetsAtom == Atom.None)
        {
            return Task.CompletedTask;
        }

        var clipboardManager = XLib.XGetSelectionOwner(this.x11.Display, this.x11.ClipboardManagerAtom);
        if (clipboardManager == IntPtr.Zero)
        {
            return Task.CompletedTask;
        }

        if (this.storeAtomSource is null || this.storeAtomSource.Task.IsCompleted)
        {
            this.storeAtomSource = new();
        }

        this.LogStoringAtomsInClipbaordManager();

        XLib.XChangeProperty(
            this.x11.Display,
            this.windowHandle,
            this.x11.CustomSaveTargetsAtom,
            Atom.Atom,
            32,
            XPropertyMode.Replace,
            this.atoms,
            this.atoms.Length);

        XLib.XConvertSelection(
            this.x11.Display,
            this.x11.ClipboardManagerAtom,
            this.x11.SaveTargetsAtom,
            this.x11.CustomSaveTargetsAtom,
            this.windowHandle,
            IntPtr.Zero);

        return this.storeAtomSource.Task;
    }

    private void OnEvent(ref XEvent xEvent)
    {
        if (xEvent.Type == XEventName.SelectionClear)
        {
            this.LogSelectionClearEventReceived();
            this.storeAtomSource?.TrySetResult(true);
            return;
        } else if (xEvent.Type == XEventName.SelectionRequest)
        {
            this.LogSelectionRequestEventReceived();
            this.OnSelectionRequest(xEvent.SelectionRequestEvent);
        } else if (xEvent.Type == XEventName.SelectionNotify &&
            xEvent.SelectionEvent.Selection == this.x11.ClipboardAtom)
        {
            this.LogSelectionNotifyEventReceived();
            this.OnClipboardSelectionNotify(xEvent.SelectionEvent);
        }
    }

    private void OnSelectionRequest(XSelectionRequestEvent request)
    {
        var response = new XEvent
        {
            SelectionEvent =
                {
                    Type = XEventName.SelectionNotify,
                    SendEvent = 1,
                    Display = this.x11.Display.DangerousGetHandle(),
                    Selection = request.Selection,
                    Target = request.Target,
                    Requestor = request.Requestor,
                    Time = request.Time,
                    Property = Atom.None
                }
        };

        if (request.Selection == this.x11.ClipboardAtom)
        {
            response.SelectionEvent.Property = WriteTargetToProperty(
                request.Target, request.Requestor, request.Property);
        }

        XLib.XSendEvent(
            this.x11.Display, request.Requestor, false, new IntPtr((int)XEventMask.NoEventMask), ref response);
    }

    private unsafe void OnClipboardSelectionNotify(XSelectionEvent sel)
    {
        if (sel.Property == Atom.None)
        {
            this.LogX11EventDoesNotHaveProperty();
            requestedFormatsSource?.TrySetResult(null);
            requestedDataSource?.TrySetResult(null);
        }

        XLib.XGetWindowProperty(
            this.x11.Display,
            this.windowHandle,
            sel.Property,
            0,
            0x7FFFFFFF,
            true,
            Atom.None,
            out var actualTypeAtom,
            out var actualFormat,
            out var numItems,
            out var _,
            out var prop);

        if (numItems == 0)
        {
            this.LogTextSelectedOrNoFormatAvailable();
            this.requestedFormatsSource?.TrySetResult(null);
            this.requestedDataSource?.TrySetResult(null);
        } else
        {
            if (sel.Property == this.x11.TargetsAtom)
            {
                if (actualFormat != 32)
                {
                    this.requestedFormatsSource?.TrySetResult(null);
                } else
                {
                    var formats = new IntPtr[numItems];
                    Marshal.Copy(prop, formats, 0, formats.Length);
                    this.requestedFormatsSource?.TrySetResult(formats.Select(f => (Atom)f).ToArray());
                    this.LogRespondedToFormatsRequest();
                }
            } else if (this.GetStringEncoding(actualTypeAtom) is { } textEncondig)
            {
                var text = textEncondig.GetString((byte*)prop.ToPointer(), numItems.ToInt32());
                this.requestedDataSource?.TrySetResult(text);
                this.LogRespondedToDataRequest();
            } else if (actualTypeAtom == this.x11.IncrAtom)
            {
                this.requestedDataSource?.TrySetResult(null);
                this.LogCouldNotGetSelectedText();
            } else
            {
                var data = new byte[(int)numItems * (actualFormat / 8)];
                Marshal.Copy(prop, data, 0, data.Length);
                this.requestedDataSource?.TrySetResult(data);
                this.LogRespondedToDataRequestFallback();
            }
        }

        XLib.XFree(prop);
    }

    private unsafe Atom WriteTargetToProperty(Atom target, IntPtr window, Atom property)
    {
        this.LogWritingTargetToProperty((ulong)target, (ulong)property);

        if (target == this.x11.TargetsAtom)
        {
            XLib.XChangeProperty(
                this.x11.Display,
                window,
                property,
                Atom.Atom,
                32,
                XPropertyMode.Replace,
                this.atoms,
                this.atoms.Length);

            return property;
        } else if (target == this.x11.SaveTargetsAtom && this.x11.SaveTargetsAtom != Atom.None)
        {
            return property;
        } else if (this.GetStringEncoding(target) is { } textEnconding)
        {
            if (this.storedText is null)
            {
                return Atom.None;
            }

            var data = textEnconding.GetBytes(this.storedText);

            fixed (void* pdata = data)
            {
                XLib.XChangeProperty(
                    this.x11.Display, window, property, target, 8, XPropertyMode.Replace, pdata, data.Length);
            }

            return property;
        } else if (target == this.x11.MultipleAtom && this.x11.MultipleAtom != Atom.None)
        {
            XLib.XGetWindowProperty(
                this.x11.Display,
                window,
                property,
                IntPtr.Zero,
                new IntPtr(0x7fffffff),
                false,
                this.x11.AtomPairAtom,
                out _,
                out var actualFormat,
                out var numItems,
                out _,
                out var prop);

            if (numItems == 0)
            {
                return Atom.None;
            }

            if (actualFormat == 32)
            {
                var data = (Atom*)prop.ToPointer();
                for (var c = 0; c < numItems.ToInt32(); c += 2)
                {
                    var subTarget = data[c];
                    var subProp = data[c + 1];
                    var converted = WriteTargetToProperty(subTarget, window, subProp);
                    data[c + 1] = converted;
                }

                XLib.XChangeProperty(
                    this.x11.Display,
                    window,
                    property,
                    this.x11.AtomPairAtom,
                    32,
                    XPropertyMode.Replace,
                    prop.ToPointer(), numItems.ToInt32());
            }

            XLib.XFree(prop);

            return property;
        } else
        {
            return Atom.None;
        }
    }

    private Encoding? GetStringEncoding(Atom atom) =>
        atom switch
        {
            var a when a == Atom.String || a == this.x11.OemTextAtom => Encoding.ASCII,
            var a when a == this.x11.Utf8StringAtom => Encoding.UTF8,
            var a when a == this.x11.Utf16StringAtom => Encoding.Unicode,
            _ => null
        };

    [LoggerMessage(LogLevel.Debug, "Getting the text from the clipboard")]
    private partial void LogGettingTextFromClipboard();

    [LoggerMessage(LogLevel.Debug, "Clipboard selection owner is absent, so there's no text to get")]
    private partial void LogClipbaordSelectionOwnerAbsent();

    [LoggerMessage(LogLevel.Debug, "Getting text in format {Atom}")]
    private partial void LogGettingText(ulong atom);

    [LoggerMessage(LogLevel.Debug, "Setting the text into the clipboard")]
    private partial void LogSettingTextIntoClipboard();

    [LoggerMessage(LogLevel.Debug, "Sending a format request to X11")]
    private partial void LogSendingForwardRequestToX11();

    [LoggerMessage(LogLevel.Debug, "Sending a data request to X11")]
    private partial void LogSendingDataRequestToX11();

    [LoggerMessage(LogLevel.Debug, "Storing atoms in the clipboard manager of X11")]
    private partial void LogStoringAtomsInClipbaordManager();

    [LoggerMessage(LogLevel.Debug, "Selection clear event received from X11")]
    private partial void LogSelectionClearEventReceived();

    [LoggerMessage(LogLevel.Debug, "Selection request event received from X11")]
    private partial void LogSelectionRequestEventReceived();

    [LoggerMessage(LogLevel.Debug, "Selection notify event received from X11")]
    private partial void LogSelectionNotifyEventReceived();

    [LoggerMessage(LogLevel.Debug, "X11 event doesn't have a property")]
    private partial void LogX11EventDoesNotHaveProperty();

    [LoggerMessage(LogLevel.Debug, "No text is selected or no formats are available")]
    private partial void LogTextSelectedOrNoFormatAvailable();

    [LoggerMessage(LogLevel.Debug, "Responded to the formats request")]
    private partial void LogRespondedToFormatsRequest();

    [LoggerMessage(LogLevel.Debug, "Responded to the data request using the data format")]
    private partial void LogRespondedToDataRequest();

    [LoggerMessage(LogLevel.Debug, "Could not get selected text")]
    private partial void LogCouldNotGetSelectedText();

    [LoggerMessage(LogLevel.Debug, "Responded to the data request using a fallback")]
    private partial void LogRespondedToDataRequestFallback();

    [LoggerMessage(LogLevel.Debug, "Writing target {Target} to property {Property}")]
    private partial void LogWritingTargetToProperty(ulong target, ulong property);
}

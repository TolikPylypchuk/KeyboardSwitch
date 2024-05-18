using System.Reactive.Concurrency;
using System.Text;

using static KeyboardSwitch.Core.Constants;

namespace KeyboardSwitch.Linux.Services;

internal sealed class XClipboardService : IClipboardService
{
    private readonly X11Service x11;
    private readonly IScheduler scheduler;
    private readonly ILogger<XClipboardService> logger;

    private readonly IntPtr windowHandle;
    private readonly Atom[] atoms;
    private readonly Atom[] textAtoms;

    private string? storedText;

    private TaskCompletionSource<bool>? storeAtomSource;
    private TaskCompletionSource<Atom[]?>? requestedFormatsSource;
    private TaskCompletionSource<object?>? requestedDataSource;

    public XClipboardService(X11Service x11, IScheduler scheduler, ILogger<XClipboardService> logger)
    {
        this.x11 = x11;
        this.scheduler = scheduler;
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

    public async Task<string?> GetText()
    {
        this.logger.LogDebug("Getting the text from the clipboard");

        if (XLib.XGetSelectionOwner(this.x11.Display, this.x11.ClipboardAtom) == IntPtr.Zero)
        {
            this.logger.LogDebug("Clipboard selection owner is absent, so there's no text to get");
            return null;
        }

        var response = await this.SendFormatRequest();

        var target = response is not null
            ? textAtoms.FirstOrDefault(f => response.Contains(f))
            : Atom.None;

        this.logger.LogDebug("Getting text in format {Atom}", (ulong)target);

        var data = await this.SendDataRequest(target != Atom.None ? target : this.x11.Utf8StringAtom);
        return data?.ToString();
    }

    public Task SetText(string text)
    {
        this.logger.LogDebug("Setting the text into the clipboard");

        this.storedText = text;
        XLib.XSetSelectionOwner(this.x11.Display, this.x11.ClipboardAtom, this.windowHandle, IntPtr.Zero);

        return this.StoreAtomsInClipboardManager();
    }

    public async Task<IAsyncDisposable> SaveClipboardState()
    {
        var savedText = await this.GetText();
        var saveTime = this.scheduler.Now;

        return new AsyncDisposable(async () =>
        {
            if (!String.IsNullOrEmpty(savedText) && this.scheduler.Now - saveTime < MaxClipboardRestoreDuration)
            {
                await this.scheduler.Sleep(TimeSpan.FromMilliseconds(50));
                await this.SetText(savedText);
            }
        });
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

        this.logger.LogDebug("Sending a format request to X11");

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

        this.logger.LogDebug("Sending a data request to X11");

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

        this.logger.LogDebug("Storing atoms in the clipboard manager of X11");

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
            this.logger.LogDebug("Selection clear event received from X11");
            this.storeAtomSource?.TrySetResult(true);
            return;
        } else if (xEvent.Type == XEventName.SelectionRequest)
        {
            this.logger.LogDebug("Selection request event received from X11");
            this.OnSelectionRequest(xEvent.SelectionRequestEvent);
        } else if (xEvent.Type == XEventName.SelectionNotify &&
            xEvent.SelectionEvent.Selection == this.x11.ClipboardAtom)
        {
            this.logger.LogDebug("Selection notify event received from X11");
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
            this.logger.LogDebug("X11 event doesn't have a property");
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
            this.logger.LogDebug("No text is selected or no formats are available");
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
                    this.logger.LogDebug("Responded to the formats request");
                }
            } else if (this.GetStringEncoding(actualTypeAtom) is { } textEncondig)
            {
                var text = textEncondig.GetString((byte*)prop.ToPointer(), numItems.ToInt32());
                this.requestedDataSource?.TrySetResult(text);
                this.logger.LogDebug("Responded to the data request using the data format");
            } else if (actualTypeAtom == this.x11.IncrAtom)
            {
                this.requestedDataSource?.TrySetResult(null);
                this.logger.LogDebug("Could not get selected text");
            } else
            {
                var data = new byte[(int)numItems * (actualFormat / 8)];
                Marshal.Copy(prop, data, 0, data.Length);
                this.requestedDataSource?.TrySetResult(data);
                this.logger.LogDebug("Responded to the data request using a fallback");
            }
        }

        XLib.XFree(prop);
    }

    private unsafe Atom WriteTargetToProperty(Atom target, IntPtr window, Atom property)
    {
        this.logger.LogDebug("Writing target {Target} to property {Property}", (ulong)target, (ulong)property);

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
}

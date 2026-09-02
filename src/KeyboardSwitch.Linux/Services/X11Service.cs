namespace KeyboardSwitch.Linux.Services;

internal delegate void XEventHandler(ref XEvent xEvent);

internal sealed partial class X11Service : DisposableService
{
    private readonly List<IntPtr> windows = [];
    private readonly Dictionary<IntPtr, XEventHandler> eventHandlers = [];
    private readonly ILogger<X11Service> logger;

    public X11Service(ILogger<X11Service> logger)
    {
        this.logger = logger;
        this.Display = this.OpenXDisplay();

        // For some reason without setting the synchronous mode on, switching the current keyboard layout
        // doesn't work when xsel integration is enabled
        XLib.XSynchronize(this.Display, true);

        this.AtomPairAtom = XLib.XInternAtom(this.Display, "ATOM_PAIR", true);
        this.ClipboardAtom = XLib.XInternAtom(this.Display, "CLIPBOARD", false);
        this.ClipboardManagerAtom = XLib.XInternAtom(this.Display, "CLIPBOARD_MANAGER", true);
        this.IncrAtom = XLib.XInternAtom(this.Display, "INCR", true);
        this.MultipleAtom = XLib.XInternAtom(this.Display, "MULTIPLE", true);
        this.OemTextAtom = XLib.XInternAtom(this.Display, "OEMTEXT", true);
        this.SaveTargetsAtom = XLib.XInternAtom(this.Display, "SAVE_TARGETS", true);
        this.TargetsAtom = XLib.XInternAtom(this.Display, "TARGETS", true);
        this.Utf8StringAtom = XLib.XInternAtom(this.Display, "UTF8_STRING", false);
        this.Utf16StringAtom = XLib.XInternAtom(this.Display, "UTF16_STRING", true);

        this.CustomSaveTargetsAtom = XLib.XInternAtom(
            this.Display, "KEYBOARD_SWITCH_SAVE_TARGETS_PROPERTY_ATOM", false);

        this.LogAtom("ATOM_PAIR", (ulong)this.AtomPairAtom);
        this.LogAtom("CLIPBOARD", (ulong)this.ClipboardAtom);
        this.LogAtom("CLIPBOARD_MANAGER", (ulong)this.ClipboardManagerAtom);
        this.LogAtom("INCR", (ulong)this.IncrAtom);
        this.LogAtom("MULTIPLE", (ulong)this.MultipleAtom);
        this.LogAtom("OEM_TEXT", (ulong)this.OemTextAtom);
        this.LogAtom("SAVE_TARGETS", (ulong)this.SaveTargetsAtom);
        this.LogAtom("STRING", (ulong)Atom.String);
        this.LogAtom("TARGETS", (ulong)this.TargetsAtom);
        this.LogAtom("UTF8_STRING", (ulong)this.Utf8StringAtom);
        this.LogAtom("UTF16_STRING", (ulong)this.Utf16StringAtom);
        this.LogAtom("KEYBOARD_SWITCH_SAVE_TARGETS_PROPERTY_ATOM", (ulong)this.CustomSaveTargetsAtom);
    }

    public XDisplayHandle Display { get; }

    public Atom AtomPairAtom { get; }
    public Atom ClipboardAtom { get; }
    public Atom ClipboardManagerAtom { get; }
    public Atom IncrAtom { get; }
    public Atom MultipleAtom { get; }
    public Atom OemTextAtom { get; }
    public Atom SaveTargetsAtom { get; }
    public Atom TargetsAtom { get; }
    public Atom Utf8StringAtom { get; }
    public Atom Utf16StringAtom { get; }

    public Atom CustomSaveTargetsAtom { get; }

    public IntPtr CreateWindow()
    {
        var win = XLib.XCreateSimpleWindow(
            this.Display,
            XLib.XDefaultRootWindow(this.Display),
            0,
            0,
            1,
            1,
            0,
            IntPtr.Zero,
            IntPtr.Zero);

        this.windows.Add(win);
        return win;
    }

    public void AddEventHandler(IntPtr window, XEventHandler eventHandler) =>
        this.eventHandlers.Add(window, eventHandler);

    public bool TryGetEventHandler(IntPtr window, out XEventHandler eventHandler) =>
        this.eventHandlers.TryGetValue(window, out eventHandler!);

    protected override void Dispose(bool disposing)
    {
        foreach (var window in this.windows)
        {
            XLib.XDestroyWindow(this.Display, window);
        }

        this.Display.Dispose();
    }

    private XDisplayHandle OpenXDisplay()
    {
        XLib.XInitThreads();
        XLib.XkbIgnoreExtension(false);

        int major = XLib.XkbMajorVersion;
        int minor = XLib.XkbMinorVersion;

        var display = XLib.XkbOpenDisplay(String.Empty, out _, out _, ref major, ref minor, out var result);
        this.ValidateXOpenDisplayResult(result);

        return display;
    }

    private void ValidateXOpenDisplayResult(XOpenDisplayResult result)
    {
        switch (result)
        {
            case XOpenDisplayResult.BadLibraryVersion:
                throw new XException("Bad X11 version");
            case XOpenDisplayResult.ConnectionRefused:
                throw new XException("Connection to X server refused");
            case XOpenDisplayResult.NonXkbServer:
                throw new XException("XKB not present");
            case XOpenDisplayResult.BadServerVersion:
                throw new XException("Bad X11 server version");
        }
    }

    [LoggerMessage(LogLevel.Debug, "{AtomName}: {Atom}")]
    private partial void LogAtom(string atomName, ulong atom);
}

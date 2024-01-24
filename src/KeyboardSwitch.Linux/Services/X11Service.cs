namespace KeyboardSwitch.Linux.Services;

internal sealed class X11Service : Disposable
{
    public delegate void EventHandler(ref XEvent xEvent);

    private readonly Dictionary<IntPtr, EventHandler> eventHandlers = [];

    public X11Service(ILogger<X11Service> logger)
    {
        this.Display = this.OpenXDisplay();

        this.AtomPairAtom = XLib.XInternAtom(this.Display, "ATOM_PAIR", true);
        this.ClipboardAtom = XLib.XInternAtom(this.Display, "CLIPBOARD", true);
        this.ClipboardManagerAtom = XLib.XInternAtom(this.Display, "CLIPBOARD_MANAGER", true);
        this.IncrAtom = XLib.XInternAtom(this.Display, "INCR", true);
        this.MultipleAtom = XLib.XInternAtom(this.Display, "MULTIPLE", true);
        this.OemTextAtom = XLib.XInternAtom(this.Display, "OEMTEXT", true);
        this.SaveTargetsAtom = XLib.XInternAtom(this.Display, "SAVE_TARGETS", true);
        this.TargetsAtom = XLib.XInternAtom(this.Display, "TARGETS", true);
        this.Utf8StringAtom = XLib.XInternAtom(this.Display, "UTF8_STRING", true);
        this.Utf16StringAtom = XLib.XInternAtom(this.Display, "UTF16_STRING", true);

        this.CustomSaveTargetsAtom = XLib.XInternAtom(
            this.Display, "KEYBOARD_SWITCH_SAVE_TARGETS_PROPERTY_ATOM", false);

        logger.LogDebug("ATOM_PAIR: {Atom}", (ulong)this.AtomPairAtom);
        logger.LogDebug("CLIPBOARD: {Atom}", (ulong)this.ClipboardAtom);
        logger.LogDebug("CLIPBOARD_MANAGER: {Atom}", (ulong)this.ClipboardManagerAtom);
        logger.LogDebug("INCR: {Atom}", (ulong)this.IncrAtom);
        logger.LogDebug("MULTIPLE: {Atom}", (ulong)this.MultipleAtom);
        logger.LogDebug("OEM_TEXT: {Atom}", (ulong)this.OemTextAtom);
        logger.LogDebug("SAVE_TARGETS: {Atom}", (ulong)this.SaveTargetsAtom);
        logger.LogDebug("STRING: {Atom}", (ulong)Atom.String);
        logger.LogDebug("TARGETS: {Atom}", (ulong)this.TargetsAtom);
        logger.LogDebug("UTF8_STRING: {Atom}", (ulong)this.Utf8StringAtom);
        logger.LogDebug("UTF16_STRING: {Atom}", (ulong)this.Utf16StringAtom);
        logger.LogDebug("KEYBOARD_SWITCH_SAVE_TARGETS_PROPERTY_ATOM: {Atom}", (ulong)this.CustomSaveTargetsAtom);
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

    public void AddEventHandler(IntPtr window, EventHandler eventHandler) =>
        this.eventHandlers.Add(window, eventHandler);

    public bool TryGetEventHandler(IntPtr window, out EventHandler eventHandler) =>
        this.eventHandlers.TryGetValue(window, out eventHandler!);

    protected override void Dispose(bool disposing) =>
        this.Display.Dispose();

    private XDisplayHandle OpenXDisplay()
    {
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
}

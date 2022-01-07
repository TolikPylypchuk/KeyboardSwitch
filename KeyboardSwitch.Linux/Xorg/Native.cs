namespace KeyboardSwitch.Linux.Xorg;

internal static class Native
{
    public const int XkbMajorVersion = 1;
    public const int XkbMinorVersion = 0;
    public const uint XkbUseCoreKbd = 0x0100;
    public const int XkbNumVirtualMods = 16;
    public const int XkbNumIndicators = 32;
    public const int XkbNumKbdGroups = 4;
    public const int XkbMaxShiftLevel = 63;

    private const string X11 = "libX11.so.6";

    [DllImport(X11)]
    public static extern bool XkbIgnoreExtension(bool ignore);

    [DllImport(X11, CharSet = CharSet.Unicode)]
    public static extern XDisplayHandle XkbOpenDisplay(
        string display,
        out int eventCode,
        out int errorCode,
        ref int major,
        ref int minor,
        out XOpenDisplayResult result);

    [DllImport(X11)]
    public static extern bool XkbQueryExtension(
        XDisplayHandle display,
        out int opCode,
        out int eventCode,
        out int errorCode,
        ref int major,
        ref int minor);

    [DllImport(X11)]
    public static extern XHandle XkbAllocKeyboard();

    [DllImport(X11)]
    public static extern bool XFree(IntPtr handle);

    [DllImport(X11)]
    public static extern Status XkbGetControls(XDisplayHandle display, XControlsDetailMask which, XHandle desc);

    [DllImport(X11)]
    public static extern void XkbFreeControls(XHandle desc, XControlsDetailMask which, bool freeMap);

    [DllImport(X11)]
    public static extern Status XkbGetNames(XDisplayHandle display, XNamesComponentMask which, XHandle desc);

    [DllImport(X11)]
    public static extern Status XkbFreeNames(XHandle desc, XNamesComponentMask which, bool freeMap);

    [DllImport(X11)]
    public static extern XHandle XGetAtomName(XDisplayHandle dispaly, Atom atom);

    [DllImport(X11)]
    public static extern Status XkbGetState(
        XDisplayHandle display,
        uint deviceSpec,
        [In, Out] ref XkbState state);

    [DllImport(X11)]
    public static extern Status XkbSelectEventDetails(
        XDisplayHandle display,
        uint deviceId,
        XkbEventType eventType,
        XStateMask affect,
        XStateMask details);

    [DllImport(X11)]
    public static extern IntPtr XkbGetMap(
        XDisplayHandle display,
        XkbMapComponentMask which,
        XkbKeyboardSpec deviceSpec);

    [DllImport(X11)]
    public static extern void XkbFreeClientMap(IntPtr map, XkbMapComponentMask which, bool freeMap);

    [DllImport(X11)]
    public static extern ulong XkbKeycodeToKeysym(XDisplayHandle display, KeyCode keyCode, int group, int level);
}

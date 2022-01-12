namespace KeyboardSwitch.Linux.X11;

internal static class Native
{
    public const int XkbMajorVersion = 1;
    public const int XkbMinorVersion = 0;

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
    public static extern XStatus XCloseDisplay(IntPtr display);

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
    public static extern XStatus XFree(IntPtr handle);

    [DllImport(X11)]
    public static extern XStatus XkbGetControls(XDisplayHandle display, XControlsDetailMask which, XHandle desc);

    [DllImport(X11)]
    public static extern void XkbFreeControls(XHandle desc, XControlsDetailMask which, bool freeMap);

    [DllImport(X11)]
    public static extern XStatus XkbGetNames(XDisplayHandle display, XNamesComponentMask which, XHandle desc);

    [DllImport(X11)]
    public static extern XStatus XkbFreeNames(XHandle desc, XNamesComponentMask which, bool freeMap);

    [DllImport(X11)]
    public static extern XHandle XGetAtomName(XDisplayHandle dispaly, Atom atom);

    [DllImport(X11)]
    public static extern XStatus XkbGetState(
        XDisplayHandle display,
        XkbKeyboardSpec deviceSpec,
        [In, Out] ref XkbState state);

    [DllImport(X11)]
    public static extern XStatus XkbSelectEventDetails(
        XDisplayHandle display,
        XkbKeyboardSpec deviceId,
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
    public static extern XKeySym XkbKeycodeToKeysym(XDisplayHandle display, byte keyCode, int group, int level);

    [DllImport(X11)]
    public static extern int XkbLockGroup(XDisplayHandle display, XkbKeyboardSpec deviceId, uint group);
}

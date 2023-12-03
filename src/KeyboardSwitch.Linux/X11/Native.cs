namespace KeyboardSwitch.Linux.X11;

internal static partial class Native
{
    public const int XkbMajorVersion = 1;
    public const int XkbMinorVersion = 0;

    public const int XkbNumVirtualMods = 16;
    public const int XkbNumIndicators = 32;

    public const int XkbNumKbdGroups = 4;
    public const int XkbMaxShiftLevel = 63;

    private const string X11 = "libX11.so.6";

    [LibraryImport(X11)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool XkbIgnoreExtension([MarshalAs(UnmanagedType.I1)] bool ignore);

    [LibraryImport(X11, StringMarshalling = StringMarshalling.Utf8)]
    public static partial XDisplayHandle XkbOpenDisplay(
        string display,
        out int eventCode,
        out int errorCode,
        ref int major,
        ref int minor,
        out XOpenDisplayResult result);

    [LibraryImport(X11)]
    public static partial XStatus XCloseDisplay(IntPtr display);

    [LibraryImport(X11)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool XkbQueryExtension(
        XDisplayHandle display,
        out int opCode,
        out int eventCode,
        out int errorCode,
        ref int major,
        ref int minor);

    [LibraryImport(X11)]
    public static partial XHandle XkbAllocKeyboard();

    [LibraryImport(X11)]
    public static partial XStatus XFree(IntPtr handle);

    [LibraryImport(X11)]
    public static partial XStatus XkbGetControls(XDisplayHandle display, XControlsDetailMask which, XHandle desc);

    [LibraryImport(X11)]
    public static partial void XkbFreeControls(
        XHandle desc,
        XControlsDetailMask which,
        [MarshalAs(UnmanagedType.I1)] bool freeMap);

    [LibraryImport(X11)]
    public static partial XStatus XkbGetNames(XDisplayHandle display, XNamesComponentMask which, XHandle desc);

    [LibraryImport(X11)]
    public static partial XStatus XkbFreeNames(
        XHandle desc,
        XNamesComponentMask which,
        [MarshalAs(UnmanagedType.I1)] bool freeMap);

    [LibraryImport(X11)]
    public static partial XHandle XGetAtomName(XDisplayHandle dispaly, Atom atom);

    [LibraryImport(X11)]
    public static partial XStatus XkbGetState(
        XDisplayHandle display,
        XkbKeyboardSpec deviceSpec,
        ref XkbState state);

    [LibraryImport(X11)]
    public static partial XStatus XkbSelectEventDetails(
        XDisplayHandle display,
        XkbKeyboardSpec deviceId,
        XkbEventType eventType,
        XStateMask affect,
        XStateMask details);

    [LibraryImport(X11)]
    public static partial IntPtr XkbGetMap(
        XDisplayHandle display,
        XkbMapComponentMask which,
        XkbKeyboardSpec deviceSpec);

    [LibraryImport(X11)]
    public static partial void XkbFreeClientMap(
        IntPtr map,
        XkbMapComponentMask which,
        [MarshalAs(UnmanagedType.I1)] bool freeMap);

    [LibraryImport(X11)]
    public static partial XKeySym XkbKeycodeToKeysym(XDisplayHandle display, byte keyCode, int group, int level);

    [LibraryImport(X11)]
    public static partial int XkbLockGroup(XDisplayHandle display, XkbKeyboardSpec deviceId, uint group);
}

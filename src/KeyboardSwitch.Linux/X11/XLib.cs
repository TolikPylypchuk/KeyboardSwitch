namespace KeyboardSwitch.Linux.X11;

internal unsafe static partial class XLib
{
    public const int XkbMajorVersion = 1;
    public const int XkbMinorVersion = 0;

    public const int XkbNumVirtualMods = 16;
    public const int XkbNumIndicators = 32;

    public const int XkbNumKbdGroups = 4;
    public const int XkbMaxShiftLevel = 63;

    private const string X11 = "libX11.so.6";

    [LibraryImport(X11)]
    public static partial int XChangeProperty(
        XDisplayHandle display,
        IntPtr window,
        Atom property,
        Atom type,
        int format,
        XPropertyMode mode,
        [In] Atom[] data,
        int numElements);

    [LibraryImport(X11)]
    public static partial int XChangeProperty(
        XDisplayHandle display,
        IntPtr window,
        Atom property,
        Atom type,
        int format,
        XPropertyMode mode,
        void* data,
        int numElements);

    [LibraryImport(X11)]
    public static partial XStatus XCloseDisplay(IntPtr display);

    [LibraryImport(X11)]
    public static partial int XConnectionNumber(XDisplayHandle display);

    [LibraryImport(X11)]
    public static partial int XConvertSelection(
        XDisplayHandle display,
        Atom selection,
        Atom target,
        Atom property,
        IntPtr requestor,
        IntPtr time);

    [LibraryImport(X11)]
    public static partial IntPtr XCreateSimpleWindow(
        XDisplayHandle display,
        IntPtr parent,
        int x,
        int y,
        int width,
        int height,
        int borderWidth,
        IntPtr border,
        IntPtr background);

    [LibraryImport(X11)]
    public static partial IntPtr XDefaultRootWindow(XDisplayHandle display);

    [LibraryImport(X11)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool XFilterEvent(ref XEvent xEvent, IntPtr window);

    [LibraryImport(X11)]
    public static partial XStatus XFree(IntPtr handle);

    [LibraryImport(X11)]
    public static partial void XFreeEventData(XDisplayHandle display, void* cookie);

    [LibraryImport(X11)]
    public static partial int XFlush(XDisplayHandle display);

    [LibraryImport(X11)]
    public static partial XHandle XGetAtomName(XDisplayHandle dispaly, Atom atom);

    [LibraryImport(X11)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool XGetEventData(XDisplayHandle display, void* cookie);

    [LibraryImport(X11)]
    public static partial IntPtr XGetSelectionOwner(XDisplayHandle display, Atom selection);

    [LibraryImport(X11)]
    public static partial int XGetWindowProperty(
        XDisplayHandle display,
        IntPtr window,
        Atom atom,
        nint longOffset,
        nint longLength,
        [MarshalAs(UnmanagedType.I1)] bool delete,
        Atom reqType,
        out Atom actualType,
        out int actualFormat,
        out nint numItems,
        out IntPtr bytesAfter,
        out IntPtr prop);

    [LibraryImport(X11, StringMarshalling = StringMarshalling.Utf8)]
    public static partial Atom XInternAtom(
        XDisplayHandle display,
        string atomName,
        [MarshalAs(UnmanagedType.I1)] bool onlyIfExists);

    [LibraryImport(X11)]
    public static partial IntPtr XNextEvent(XDisplayHandle display, out XEvent xEvent);

    [LibraryImport(X11)]
    public static partial int XPending(XDisplayHandle display);

    [LibraryImport(X11)]
    public static partial int XSendEvent(
        XDisplayHandle display,
        IntPtr window,
        [MarshalAs(UnmanagedType.I1)] bool propagate,
        IntPtr eventMask,
        ref XEvent sendEvent);

    [LibraryImport(X11)]
    public static partial int XSetSelectionOwner(XDisplayHandle display, Atom selection, IntPtr owner, IntPtr time);

    [LibraryImport(X11)]
    public static partial IntPtr XSynchronize(XDisplayHandle display, [MarshalAs(UnmanagedType.I1)] bool on);

    [LibraryImport(X11)]
    public static partial XHandle XkbAllocKeyboard();

    [LibraryImport(X11)]
    public static partial void XkbFreeClientMap(
        IntPtr map,
        XkbMapComponentMask which,
        [MarshalAs(UnmanagedType.I1)] bool freeMap);

    [LibraryImport(X11)]
    public static partial void XkbFreeControls(
        XHandle desc,
        XControlsDetailMask which,
        [MarshalAs(UnmanagedType.I1)] bool freeMap);

    [LibraryImport(X11)]
    public static partial XStatus XkbFreeNames(
        XHandle desc,
        XNamesComponentMask which,
        [MarshalAs(UnmanagedType.I1)] bool freeMap);

    [LibraryImport(X11)]
    public static partial XStatus XkbGetControls(XDisplayHandle display, XControlsDetailMask which, XHandle desc);

    [LibraryImport(X11)]
    public static partial IntPtr XkbGetMap(
        XDisplayHandle display,
        XkbMapComponentMask which,
        XkbKeyboardSpec deviceSpec);

    [LibraryImport(X11)]
    public static partial XStatus XkbGetNames(XDisplayHandle display, XNamesComponentMask which, XHandle desc);

    [LibraryImport(X11)]
    public static partial XStatus XkbGetState(
        XDisplayHandle display,
        XkbKeyboardSpec deviceSpec,
        ref XkbState state);

    [LibraryImport(X11)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static partial bool XkbIgnoreExtension([MarshalAs(UnmanagedType.I1)] bool ignore);

    [LibraryImport(X11)]
    public static partial XKeySym XkbKeycodeToKeysym(XDisplayHandle display, byte keyCode, int group, int level);

    [LibraryImport(X11)]
    public static partial int XkbLockGroup(XDisplayHandle display, XkbKeyboardSpec deviceId, uint group);

    [LibraryImport(X11, StringMarshalling = StringMarshalling.Utf8)]
    public static partial XDisplayHandle XkbOpenDisplay(
        string display,
        out int eventCode,
        out int errorCode,
        ref int major,
        ref int minor,
        out XOpenDisplayResult result);

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
    public static partial XStatus XkbSelectEventDetails(
        XDisplayHandle display,
        XkbKeyboardSpec deviceId,
        XkbEventType eventType,
        XStateMask affect,
        XStateMask details);
}

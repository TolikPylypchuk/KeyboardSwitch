namespace KeyboardSwitch.MacOS.Native;

internal static partial class CoreGraphics
{
    private const string CoreGraphicsLib = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    [LibraryImport(CoreGraphicsLib)]
    public static partial CGEventSourceRef CGEventSourceCreate(CGEventSourceStateID stateID);

    [LibraryImport(CoreGraphicsLib)]
    public static partial CGEventRef CGEventCreateKeyboardEvent(
        CGEventSourceRef source,
        CGKeyCode virtualKey,
        [MarshalAs(UnmanagedType.I1)] bool keyDown);

    [LibraryImport(CoreGraphicsLib)]
    public static partial void CGEventSetFlags(CGEventRef @event, CGEventFlags flags);

    [LibraryImport(CoreGraphicsLib)]
    public static partial void CGEventPost(CGEventTapLocation tap, CGEventRef @event);
}

namespace KeyboardSwitch.MacOS.Native;

internal static class CoreGraphics
{
    private const string CoreGraphicsLib = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    [DllImport(CoreGraphicsLib)]
    public static extern CGEventSourceRef CGEventSourceCreate(CGEventSourceStateID stateID);

    [DllImport(CoreGraphicsLib)]
    public static extern CGEventRef CGEventCreateKeyboardEvent(
        CGEventSourceRef source,
        CGKeyCode virtualKey,
        bool keyDown);

    [DllImport(CoreGraphicsLib)]
    public static extern void CGEventSetFlags(CGEventRef @event, CGEventFlags flags);

    [DllImport(CoreGraphicsLib)]
    public static extern void CGEventPost(CGEventTapLocation tap, CGEventRef @event);
}

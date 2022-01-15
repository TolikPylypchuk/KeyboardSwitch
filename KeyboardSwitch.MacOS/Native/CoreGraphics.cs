namespace KeyboardSwitch.MacOS.Native;

internal static class CoreGraphics
{
    private const string Graphics = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    [DllImport(Graphics)]
    public static extern CGEventSourceRef CGEventSourceCreate(CGEventSourceStateID stateID);

    [DllImport(Graphics)]
    public static extern CGEventRef CGEventCreateKeyboardEvent(
        CGEventSourceRef source,
        CGKeyCode virtualKey,
        bool keyDown);

    [DllImport(Graphics)]
    public static extern void CGEventSetFlags(CGEventRef @event, CGEventFlags flags);

    [DllImport(Graphics)]
    public static extern void CGEventPost(CGEventTapLocation tap, CGEventRef @event);
}

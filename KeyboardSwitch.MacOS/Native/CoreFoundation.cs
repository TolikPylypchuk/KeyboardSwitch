namespace KeyboardSwitch.MacOS.Native;

internal static class CoreFoundation
{
    private const string Foundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [DllImport(Foundation)]
    public static extern void CFRelease(IntPtr @ref);
}

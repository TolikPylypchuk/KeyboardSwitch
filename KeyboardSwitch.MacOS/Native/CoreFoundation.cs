namespace KeyboardSwitch.MacOS.Native;

internal static class CoreFoundation
{
    private const string CoreFoundationLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [DllImport(CoreFoundationLib)]
    public static extern void CFRelease(IntPtr @ref);

    [DllImport(CoreFoundationLib)]
    public static extern long CFStringGetLength(CFStringRef @ref);

    [DllImport(CoreFoundationLib)]
    public static extern long CFStringGetMaximumSizeForEncoding(long length, CFStringEncoding encoding);

    [DllImport(CoreFoundationLib, CharSet = CharSet.Unicode)]
    public static extern bool CFStringGetCString(
        CFStringRef @ref,
        [Out] byte[] buffer,
        long length,
        CFStringEncoding encoding);

    [DllImport(CoreFoundationLib)]
    public static extern long CFArrayGetCount(CFArrayRef theArray);

    [DllImport(CoreFoundationLib)]
    public static extern IntPtr CFArrayGetValueAtIndex(CFArrayRef theArray, long index);
}

namespace KeyboardSwitch.MacOS.Native;

internal static partial class CoreFoundation
{
    private const string CoreFoundationLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [LibraryImport(CoreFoundationLib)]
    public static partial void CFRelease(IntPtr @ref);

    [LibraryImport(CoreFoundationLib)]
    public static partial CFRunLoopRef CFRunLoopGetCurrent();

    [LibraryImport(CoreFoundationLib)]
    public static partial void CFRunLoopRun();

    [LibraryImport(CoreFoundationLib)]
    public static partial void CFRunLoopStop(CFRunLoopRef rl);

    [LibraryImport(CoreFoundationLib)]
    public static partial long CFStringGetLength(CFStringRef @ref);

    [LibraryImport(CoreFoundationLib)]
    public static partial long CFStringGetMaximumSizeForEncoding(long length, CFStringEncoding encoding);

    [LibraryImport(CoreFoundationLib)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CFStringGetCString(
        CFStringRef @ref,
        [Out] byte[] buffer,
        long length,
        CFStringEncoding encoding);

    [LibraryImport(CoreFoundationLib)]
    public static partial long CFArrayGetCount(CFArrayRef theArray);

    [LibraryImport(CoreFoundationLib)]
    public static partial IntPtr CFArrayGetValueAtIndex(CFArrayRef theArray, long index);

    [LibraryImport(CoreFoundationLib)]
    public static partial IntPtr CFDataGetBytePtr(CFDataRef @ref);
}

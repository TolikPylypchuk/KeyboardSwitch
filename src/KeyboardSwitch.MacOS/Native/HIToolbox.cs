namespace KeyboardSwitch.MacOS.Native;

internal static partial class HIToolbox
{
    private const string HIToolboxLib =
        "/System/Library/Frameworks/Carbon.framework/Frameworks/HIToolbox.framework/HIToolbox";

    static HIToolbox()
    {
        TISPropertyInputSourceID = new(GetExportedConstant(HIToolboxLib, "kTISPropertyInputSourceID"));
        TISPropertyLocalizedName = new(GetExportedConstant(HIToolboxLib, "kTISPropertyLocalizedName"));
        TISPropertyUnicodeKeyLayoutData = new(GetExportedConstant(HIToolboxLib, "kTISPropertyUnicodeKeyLayoutData"));
    }

    public static CFStringRef TISPropertyInputSourceID { get; }

    public static CFStringRef TISPropertyLocalizedName { get; }

    public static CFStringRef TISPropertyUnicodeKeyLayoutData { get; }

    [LibraryImport(HIToolboxLib)]
    public static partial byte LMGetKbdType();

    [LibraryImport(HIToolboxLib)]
    public static partial TISInputSourceRef TISCopyCurrentKeyboardInputSource();

    [LibraryImport(HIToolboxLib)]
    public static partial IntPtr TISGetInputSourceProperty(TISInputSourceRef inputSource, CFStringRef propertyKey);

    [LibraryImport(HIToolboxLib)]
    public static partial CFArrayRef TISCreateInputSourceList(
        CFDictionaryRef properties,
        [MarshalAs(UnmanagedType.I1)] bool includeAllInstalled);

    [LibraryImport(HIToolboxLib)]
    public static partial OSStatus TISSelectInputSource(TISInputSourceRef inputSource);
}

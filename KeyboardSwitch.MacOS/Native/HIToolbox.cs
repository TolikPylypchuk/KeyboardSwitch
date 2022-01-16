namespace KeyboardSwitch.MacOS.Native;

internal static class HIToolbox
{
    private const string HIToolboxLib =
        "/System/Library/Frameworks/Carbon.framework/Frameworks/HIToolbox.framework/HIToolbox";

    [DllImport(HIToolboxLib)]
    public static extern byte LMGetKbdType();

    [DllImport(HIToolboxLib)]
    public static extern TISInputSourceRef TISCopyCurrentKeyboardInputSource();

    [DllImport(HIToolboxLib)]
    public static extern IntPtr TISGetInputSourceProperty(TISInputSourceRef inputSource, CFStringRef propertyKey);

    [DllImport(HIToolboxLib)]
    public static extern CFArrayRef TISCreateInputSourceList(CFDictionaryRef properties, bool includeAllInstalled);

    [DllImport(HIToolboxLib)]
    public static extern OSStatus TISSelectInputSource(TISInputSourceRef inputSource);

    public static CFStringRef GetTISPropertyInputSourceID() =>
        new(GetExportedConstant(HIToolboxLib, "kTISPropertyInputSourceID"));

    public static CFStringRef GetTISPropertyLocalizedName() =>
        new(GetExportedConstant(HIToolboxLib, "kTISPropertyLocalizedName"));

    public static CFStringRef GetTISPropertyUnicodeKeyLayoutData() =>
        new(GetExportedConstant(HIToolboxLib, "kTISPropertyUnicodeKeyLayoutData"));
}

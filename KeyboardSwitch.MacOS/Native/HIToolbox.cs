namespace KeyboardSwitch.MacOS.Native;

internal static class HIToolbox
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
}

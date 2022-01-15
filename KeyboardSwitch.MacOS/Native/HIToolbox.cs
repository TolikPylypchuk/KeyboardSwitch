namespace KeyboardSwitch.MacOS.Native;

internal static class HIToolbox
{
    private const string HIToolboxLib =
        "/System/Library/Frameworks/Carbon.framework/Frameworks/HIToolbox.framework/HIToolbox";

    [DllImport(HIToolboxLib)]
    public static extern TISInputSourceRef TISCopyCurrentKeyboardInputSource();

    [DllImport(HIToolboxLib)]
    public static extern CFStringRef TISGetInputSourceProperty(TISInputSourceRef inputSource, CFStringRef propertyKey);

    [DllImport(HIToolboxLib)]
    public static extern CFArrayRef TISCreateInputSourceList(CFDictionaryRef properties, bool includeAllInstalled);

    [DllImport(HIToolboxLib)]
    public static extern int TISSelectInputSource(TISInputSourceRef inputSource);

    public static CFStringRef GetTISPropertyInputSourceID() =>
        new(GetExportedConstant(HIToolboxLib, "kTISPropertyInputSourceID"));

    public static CFStringRef GetTISPropertyLocalizedName() =>
        new(GetExportedConstant(HIToolboxLib, "kTISPropertyLocalizedName"));
}

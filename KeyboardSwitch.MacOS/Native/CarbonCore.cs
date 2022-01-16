namespace KeyboardSwitch.MacOS.Native;

internal static class CarbonCore
{
    private const string CarbonCoreLib =
        "/System/Library/Frameworks/CoreServices.framework/Frameworks/CarbonCore.framework/CarbonCore";

    [DllImport(CarbonCoreLib)]
    public static extern OSStatus UCKeyTranslate(
        IntPtr keyLayoutPtr,
        CGKeyCode virtualKeyCode,
        UCKeyAction keyAction,
        uint modifierKeyState,
        int keyboardType,
        UCKeyTranslateOptionBits keyTranslateOptions,
        ref uint deadKeyState,
        ulong maxStringLength,
        out ulong actualStringLength,
        [Out] byte[] unicodeString);
}

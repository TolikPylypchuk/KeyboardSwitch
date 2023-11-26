namespace KeyboardSwitch.MacOS.Native;

internal static partial class CarbonCore
{
    private const string CarbonCoreLib =
        "/System/Library/Frameworks/CoreServices.framework/Frameworks/CarbonCore.framework/CarbonCore";

    [LibraryImport(CarbonCoreLib)]
    public static partial OSStatus UCKeyTranslate(
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

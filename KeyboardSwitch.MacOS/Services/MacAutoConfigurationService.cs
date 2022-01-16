namespace KeyboardSwitch.MacOS.Services;

internal class MacAutoConfigurationService : AutoConfigurationServiceBase
{
    private const int MaxStringLength = 255;

    private readonly ImmutableList<CGKeyCode> KeyCodesToMap = new List<CGKeyCode>
    {
        CGKeyCode.AnsiQ,
        CGKeyCode.AnsiW,
        CGKeyCode.AnsiE,
        CGKeyCode.AnsiR,
        CGKeyCode.AnsiT,
        CGKeyCode.AnsiY,
        CGKeyCode.AnsiU,
        CGKeyCode.AnsiI,
        CGKeyCode.AnsiO,
        CGKeyCode.AnsiP,
        CGKeyCode.AnsiLeftBracket,
        CGKeyCode.AnsiRightBracket,
        CGKeyCode.AnsiA,
        CGKeyCode.AnsiS,
        CGKeyCode.AnsiD,
        CGKeyCode.AnsiF,
        CGKeyCode.AnsiG,
        CGKeyCode.AnsiH,
        CGKeyCode.AnsiJ,
        CGKeyCode.AnsiK,
        CGKeyCode.AnsiL,
        CGKeyCode.AnsiSemicolon,
        CGKeyCode.AnsiQuote,
        CGKeyCode.AnsiZ,
        CGKeyCode.AnsiX,
        CGKeyCode.AnsiC,
        CGKeyCode.AnsiV,
        CGKeyCode.AnsiB,
        CGKeyCode.AnsiN,
        CGKeyCode.AnsiM,
        CGKeyCode.AnsiComma,
        CGKeyCode.AnsiPeriod,
        CGKeyCode.AnsiSlash,
        CGKeyCode.AnsiBackslash,
        CGKeyCode.AnsiGrave,
        CGKeyCode.Ansi1,
        CGKeyCode.Ansi2,
        CGKeyCode.Ansi3,
        CGKeyCode.Ansi4,
        CGKeyCode.Ansi5,
        CGKeyCode.Ansi6,
        CGKeyCode.Ansi7,
        CGKeyCode.Ansi8,
        CGKeyCode.Ansi9,
        CGKeyCode.Ansi0,
        CGKeyCode.AnsiMinus,
        CGKeyCode.AnsiEqual
    }.ToImmutableList();

    protected override IEnumerable<List<KeyToCharResult>> GetChars(List<string> layoutIds)
    {
        using var sources = HIToolbox.TISCreateInputSourceList(new CFDictionaryRef(), includeAllInstalled: false);
        long count = CoreFoundation.CFArrayGetCount(sources);

        var sourcesByName = Enumerable.Range(0, (int)count)
            .Select(i => new TISInputSourceRef(CoreFoundation.CFArrayGetValueAtIndex(sources, i)))
            .Where(IsKeyboardInputSource)
            .ToDictionary(GetInputSourceName, source => source);

        using var layoutDataProperty = HIToolbox.GetTISPropertyUnicodeKeyLayoutData();

        return KeyCodesToMap.Select(keyCode =>
                this.GetCharsFromKey(keyCode, shift: false, alt: false, layoutDataProperty, layoutIds, sourcesByName))
            .Concat(KeyCodesToMap.Select(keyCode =>
                this.GetCharsFromKey(keyCode, shift: true, alt: false, layoutDataProperty, layoutIds, sourcesByName)))
            .Concat(KeyCodesToMap.Select(keyCode =>
                this.GetCharsFromKey(keyCode, shift: false, alt: true, layoutDataProperty, layoutIds, sourcesByName)))
            .Concat(KeyCodesToMap.Select(keyCode =>
                this.GetCharsFromKey(keyCode, shift: true, alt: true, layoutDataProperty, layoutIds, sourcesByName)))
            .ToList();
    }

    private List<KeyToCharResult> GetCharsFromKey(
        CGKeyCode keyCode,
        bool shift,
        bool alt,
        CFStringRef layoutDataProperty,
        List<string> layoutIds,
        Dictionary<string, TISInputSourceRef> sourcesByName) =>
        layoutIds
            .Select(layoutId => this.GetCharFromKey(
                keyCode, shift, alt, layoutDataProperty, layoutId, sourcesByName[layoutId]))
            .ToList();

    private KeyToCharResult GetCharFromKey(
        CGKeyCode keyCode,
        bool shift,
        bool alt,
        CFStringRef layoutDataProperty,
        string layoutId,
        TISInputSourceRef source)
    {
        var modifierKeyState = CGEventFlags.NoMask;

        if (shift)
        {
            modifierKeyState |= CGEventFlags.MaskShift;
        }

        if (alt)
        {
            modifierKeyState |= CGEventFlags.MaskAlternate;
        }

        var unicodeString = new byte[MaxStringLength];
        uint deadKeyState = 0;

        using var layoutData = new CFDataRef(HIToolbox.TISGetInputSourceProperty(source, layoutDataProperty));

        var layoutPtr = CoreFoundation.CFDataGetBytePtr(layoutData);

        var status = CarbonCore.UCKeyTranslate(
            layoutPtr,
            keyCode,
            UCKeyAction.Down,
            ((uint)modifierKeyState >> 16) & 0xFF,
            HIToolbox.LMGetKbdType(),
            UCKeyTranslateOptionBits.TranslateNoDeadKeysMask,
            ref deadKeyState,
            MaxStringLength,
            out ulong actualStringLength,
            unicodeString);

        return status == OSStatus.NoErr && actualStringLength == 1
            ? KeyToCharResult.Success(Encoding.Unicode.GetString(unicodeString)[0], layoutId)
            : KeyToCharResult.Failure();
    }
}

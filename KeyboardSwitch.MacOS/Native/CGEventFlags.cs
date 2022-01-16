namespace KeyboardSwitch.MacOS.Native;

[Flags]
internal enum CGEventFlags : ulong
{
    MaskNonCoalesced = 0x00000100,
    MaskAlphaShift = 0x00010000,
    MaskShift = 0x00020000,
    MaskControl = 0x00040000,
    MaskAlternate = 0x00080000,
    MaskCommand = 0x00100000,
    MaskNumericPad = 0x00200000,
    MaskHelp = 0x00400000,
    MaskSecondaryFn = 0x00800000
}

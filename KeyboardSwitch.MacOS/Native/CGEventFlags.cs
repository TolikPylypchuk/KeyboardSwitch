namespace KeyboardSwitch.MacOS.Native;

[Flags]
internal enum CGEventFlags : ulong
{
    CGEventFlagMaskNonCoalesced = 0x00000100,
    CGEventFlagMaskAlphaShift = 0x00010000,
    CGEventFlagMaskShift = 0x00020000,
    CGEventFlagMaskControl = 0x00040000,
    CGEventFlagMaskAlternate = 0x00080000,
    CGEventFlagMaskCommand = 0x00100000,
    CGEventFlagMaskNumericPad = 0x00200000,
    CGEventFlagMaskHelp = 0x00400000,
    CGEventFlagMaskSecondaryFn = 0x00800000
}

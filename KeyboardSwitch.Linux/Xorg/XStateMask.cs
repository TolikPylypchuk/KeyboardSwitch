namespace KeyboardSwitch.Linux.Xorg;

[Flags]
internal enum XStateMask : ulong
{
    XkbModifierStateMask = 1L << 0,
    XkbModifierBaseMask = 1L << 1,
    XkbModifierLatchMask = 1L << 2,
    XkbModifierLockMask = 1L << 3,
    XkbGroupStateMask = 1L << 4,
    XkbGroupBaseMask = 1L << 5,
    XkbGroupLatchMask = 1L << 6,
    XkbGroupLockMask = 1L << 7,
    XkbCompatStateMask = 1L << 8,
    XkbGrabModsMask = 1L << 9,
    XkbCompatGrabModsMask = 1L << 10,
    XkbLookupModsMask = 1L << 11,
    XkbCompatLookupModsMask = 1L << 12,
    XkbPointerButtonMask = 1L << 13,
    XkbAllStateComponentsMask = 0x3FFF
}

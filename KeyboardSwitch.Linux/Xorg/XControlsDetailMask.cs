namespace KeyboardSwitch.Linux.Xorg;

[Flags]
internal enum XControlsDetailMask : ulong
{
    XkbRepeatKeysMask = 1L << 0,
    XkbSlowKeysMask = 1L << 1,
    XkbBounceKeysMask = 1L << 2,
    XkbStickyKeysMask = 1L << 3,
    XkbMouseKeysMask = 1L << 4,
    XkbMouseKeysAccelMask = 1L << 5,
    XkbAccessXKeysMask = 1L << 6,
    XkbAccessXTimeoutMask = 1L << 7,
    XkbAccessXFeedbackMask = 1L << 8,
    XkbAudibleBellMask = 1L << 9,
    XkbOverlay1Mask = 1L << 10,
    XkbOverlay2Mask = 1L << 11,
    XkbIgnoreGroupLockMask = 1L << 12,
    XkbGroupsWrapMask = 1L << 27,
    XkbInternalModsMask = 1L << 28,
    XkbIgnoreLockModsMask = 1L << 29,
    XkbPerKeyRepeatMask = 1L << 30,
    XkbControlsEnabledMask = 1L << 31,
    XkbAccessXOptionsMask = XkbStickyKeysMask | XkbAccessXFeedbackMask,
    XkbAllBooleanCtrlsMask = 0x00001FFF,
    XkbAllControlsMask = 0xF8001FFF,
    XkbAllControlEventsMask = XkbAllControlsMask
}

namespace KeyboardSwitch.Linux.X11;

[Flags]
internal enum XModifierMask
{
    ShiftMask = 1 << 0,
    LockMask = 1 << 1,
    ControlMask = 1 << 2,
    Mod1Mask = 1 << 3,
    Mod2Mask = 1 << 4,
    Mod3Mask = 1 << 5,
    Mod4Mask = 1 << 6,
    Mod5Mask = 1 << 7,
    Button1Mask = 1 << 8,
    Button2Mask = 1 << 9,
    Button3Mask = 1 << 10,
    Button4Mask = 1 << 11,
    Button5Mask = 1 << 12,
    AnyModifier = 1 << 15
}

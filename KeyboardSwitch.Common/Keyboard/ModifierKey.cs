using System;

namespace KeyboardSwitch.Common.Keyboard
{
    [Flags]
    public enum ModifierKey
    {
        None = 0,

        LeftShift = 1 << 0,
        LeftCtrl = 1 << 1,
        LeftMeta = 1 << 2,
        LeftAlt = 1 << 3,

        RightShift = 1 << 4,
        RightCtrl = 1 << 5,
        RightMeta = 1 << 6,
        RightAlt = 1 << 7,

        Shift = LeftShift | RightShift,
        Ctrl = LeftCtrl | RightCtrl,
        Meta = LeftMeta | RightMeta,
        Alt = LeftAlt | RightAlt
    }
}

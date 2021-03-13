using System;

namespace KeyboardSwitch.Common.Keyboard
{
    [Flags]
    public enum ModifierKeys
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        Meta = 8
    }
}

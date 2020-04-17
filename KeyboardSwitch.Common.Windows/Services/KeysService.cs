using KeyboardSwitch.Common.Services;

namespace KeyboardSwitch.Common.Windows.Services
{
    internal sealed class KeysService : IKeysService
    {
        public int GetVirtualKeyCode(char ch)
            => 0;

        public int GetModifierKeysCode(ModifierKeys keys)
            => (int)keys;

        public ModifierKeys? GetModifierKeyFromCode(int keyCode)
            => keyCode switch
            {
                0xA0 => ModifierKeys.Shift,
                0xA1 => ModifierKeys.Shift,
                0x10 => ModifierKeys.Shift,
                0xA2 => ModifierKeys.Ctrl,
                0xA3 => ModifierKeys.Ctrl,
                0x11 => ModifierKeys.Ctrl,
                0x12 => ModifierKeys.Alt,
                0xA4 => ModifierKeys.Alt,
                0xA5 => ModifierKeys.Alt,
                0x5B => ModifierKeys.Win,
                0x5C => ModifierKeys.Win,
                _ => null
            };
    }
}

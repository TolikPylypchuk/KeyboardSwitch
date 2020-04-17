namespace KeyboardSwitch.Common.Services
{
    public interface IKeysService
    {
        int GetVirtualKeyCode(char ch);

        int GetModifierKeysCode(ModifierKeys keys);
        ModifierKeys? GetModifierKeyFromCode(int code);
    }
}

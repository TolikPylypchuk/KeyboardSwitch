namespace KeyboardSwitch.Common.Services
{
    public interface IModiferKeysService
    {
        int GetModifierKeysCode(ModifierKeys keys);
        ModifierKeys? GetModifierKeyFromCode(int code);
    }
}

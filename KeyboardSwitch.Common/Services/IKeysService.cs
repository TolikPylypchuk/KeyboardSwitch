using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
{
    public interface IKeysService
    {
        int GetVirtualKeyCode(char ch);
    }
}

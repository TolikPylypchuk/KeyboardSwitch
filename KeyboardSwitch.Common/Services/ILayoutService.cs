using System.Collections.Generic;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
{
    public interface ILayoutService
    {
        KeyboardLayout GetCurrentKeyboardLayout();
        void SwitchCurrentLayout(SwitchDirection direction);
        List<KeyboardLayout> GetKeyboardLayouts();
    }
}

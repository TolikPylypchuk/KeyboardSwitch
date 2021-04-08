using System.Collections.Generic;

using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Core.Services
{
    public interface ILayoutService
    {
        KeyboardLayout GetCurrentKeyboardLayout();
        void SwitchCurrentLayout(SwitchDirection direction);
        List<KeyboardLayout> GetKeyboardLayouts();
    }
}

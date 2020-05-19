using System.Collections.Generic;

namespace KeyboardSwitch.Common.Services
{
    public interface ILayoutService
    {
        KeyboardLayout GetCurrentKeyboardLayout();
        void SwitchCurrentLayout(SwitchDirection direction);
        List<KeyboardLayout> GetKeyboardLayouts();
    }
}

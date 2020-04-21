using System.Collections.Generic;

namespace KeyboardSwitch.Common.Services
{
    public interface ILayoutService
    {
        KeyboardLayout GetForegroundProcessKeyboardLayout();
        void SwitchForegroundProcessLayout(SwitchDirection direction);
        List<KeyboardLayout> GetKeyboardLayouts();
    }
}

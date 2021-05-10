using System.Collections.Generic;

using KeyboardSwitch.Core.Keyboard;
using KeyboardSwitch.Core.Settings;

namespace KeyboardSwitch.Core.Services
{
    public interface ILayoutService
    {
        bool SwitchLayoutsViaKeyboardSimulation { get; }

        KeyboardLayout GetCurrentKeyboardLayout();
        void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings);
        List<KeyboardLayout> GetKeyboardLayouts();
    }
}

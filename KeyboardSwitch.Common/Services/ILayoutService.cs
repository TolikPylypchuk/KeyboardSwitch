using System.Collections.Generic;

namespace KeyboardSwitch.Common.Services
{
    public interface ILayoutService
    {
        List<KeyboardLayout> GetKeyboardLayouts();
    }
}

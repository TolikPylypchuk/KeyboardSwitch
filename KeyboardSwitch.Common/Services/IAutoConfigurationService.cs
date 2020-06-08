using System.Collections.Generic;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
{
    public interface IAutoConfigurationService
    {
        Dictionary<int, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts);
    }
}

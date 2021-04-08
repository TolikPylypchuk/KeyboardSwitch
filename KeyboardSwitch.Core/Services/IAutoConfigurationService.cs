using System.Collections.Generic;

using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Core.Services
{
    public interface IAutoConfigurationService
    {
        Dictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts);
    }
}

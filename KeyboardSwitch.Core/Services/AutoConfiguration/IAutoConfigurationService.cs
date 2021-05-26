using System.Collections.Generic;

using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Core.Services.AutoConfiguration
{
    public interface IAutoConfigurationService
    {
        Dictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts);
    }
}

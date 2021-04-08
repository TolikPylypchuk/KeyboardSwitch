using System;
using System.Collections.Generic;

using KeyboardSwitch.Core.Keyboard;
using KeyboardSwitch.Core.Services;

namespace KeyboardSwitch.Linux.Services
{
    public sealed class AutoConfigurationService : IAutoConfigurationService
    {
        public Dictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts) =>
            throw new NotImplementedException();
    }
}

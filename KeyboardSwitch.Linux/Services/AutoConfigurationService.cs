using System;
using System.Collections.Generic;

using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Common.Services;

namespace KeyboardSwitch.Linux.Services
{
    public sealed class AutoConfigurationService : IAutoConfigurationService
    {
        public Dictionary<int, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts) =>
            throw new NotImplementedException();
    }
}

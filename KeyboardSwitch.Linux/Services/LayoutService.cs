using System;
using System.Collections.Generic;

using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Common.Services;

namespace KeyboardSwitch.Linux.Services
{
    public sealed class LayoutService : ILayoutService
    {
        public KeyboardLayout GetCurrentKeyboardLayout() =>
            throw new NotImplementedException();

        public List<KeyboardLayout> GetKeyboardLayouts() =>
            throw new NotImplementedException();

        public void SwitchCurrentLayout(SwitchDirection direction) =>
            throw new NotImplementedException();
    }
}

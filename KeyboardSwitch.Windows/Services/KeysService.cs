using System;
using System.Globalization;

using KeyboardSwitch.Common.Services;

using static Vanara.PInvoke.User32;

namespace KeyboardSwitch.Windows.Services
{
    internal sealed class KeysService : IKeysService
    {
        public int GetVirtualKeyCode(char ch) =>
            VkKeyScanW(Char.ToLower(ch, CultureInfo.InvariantCulture));
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using KeyboardSwitch.Common.Services;

using Microsoft.Win32;

using static KeyboardSwitch.Common.Windows.Interop.Native;

namespace KeyboardSwitch.Common.Windows.Services
{
    public sealed class LayoutService : ILayoutService
    {
        private const string KeyboardLayoutNameRegistryKeyFormat = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts\{0}";
        private const string LayoutName = "Layout Text";

        public KeyboardLayout GetForegroundProcessKeyboardLayout()
            => this.GetThreadKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));

        public KeyboardLayout GetThreadKeyboardLayout(int threadId)
            => this.CreateKeyboardLayout((int)GetKeyboardLayout(threadId));

        public int SetThreadKeyboardLayout(int keyboardLayoutId)
            => this.SetKeyboardLayout(keyboardLayoutId, 0);

        public int SetProcessKeyboardLayout(int keyboardLayoutId)
            => this.SetKeyboardLayout(keyboardLayoutId, KlfSetForProcess);

        public List<KeyboardLayout> GetKeyboardLayouts()
        {
            int count = GetKeyboardLayoutList(0, null);
            var keyboardLayoutIds = new IntPtr[count];

            GetKeyboardLayoutList(keyboardLayoutIds.Length, keyboardLayoutIds);

            return keyboardLayoutIds
                .Select(keyboardLayoutId => this.CreateKeyboardLayout((int)keyboardLayoutId))
                .ToList();
        }

        private KeyboardLayout CreateKeyboardLayout(int keyboardLayoutId)
        {
            ushort languageId = (ushort)(keyboardLayoutId & 0xFFFF);
            ushort keyboardId = (ushort)(keyboardLayoutId >> 16);

            return new KeyboardLayout(
                keyboardLayoutId,
                languageId,
                keyboardId,
                CultureInfo.GetCultureInfo(languageId).DisplayName,
                this.GetLayoutName(keyboardLayoutId));
        }

        private int SetKeyboardLayout(int keyboardLayoutId, int flags)
            => ActivateKeyboardLayout(keyboardLayoutId, flags);

        private string GetLayoutName(int keyboardLayoutId)
        {
            SetThreadKeyboardLayout(keyboardLayoutId);

            StringBuilder name = new StringBuilder(KlNameLength);

            GetKeyboardLayoutName(name);

            using var key = Registry.LocalMachine.OpenSubKey(String.Format(KeyboardLayoutNameRegistryKeyFormat, name));

            return key?.GetValue(LayoutName)?.ToString() ?? String.Empty;
        }
    }
}

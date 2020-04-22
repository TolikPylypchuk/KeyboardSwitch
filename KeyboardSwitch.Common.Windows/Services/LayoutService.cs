using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using static KeyboardSwitch.Common.Windows.Interop.Native;

namespace KeyboardSwitch.Common.Windows.Services
{
    public sealed class LayoutService : ILayoutService
    {
        private const string KeyboardLayoutNameRegistryKeyFormat = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts\{0}";
        private const string LayoutText = "Layout Text";

        private readonly ILogger<LayoutService> logger;

        public LayoutService(ILogger<LayoutService> logger)
            => this.logger = logger;

        public KeyboardLayout GetForegroundProcessKeyboardLayout()
        {
            this.logger.LogTrace("Getting the keyboard layout of the foreground process");
            return this.GetThreadKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));
        }

        public void SwitchForegroundProcessLayout(SwitchDirection direction)
        {
            this.logger.LogTrace($"Switching the keyboard layout of the foregound process {direction.AsString()}");

            var foregroundWindowHandle = GetForegroundWindow();
            int foregroundWindowThreadId = GetWindowThreadProcessId(foregroundWindowHandle, IntPtr.Zero);

            int keyboardLayoutId = GetKeyboardLayout(foregroundWindowThreadId);

            SetThreadKeyboardLayout(keyboardLayoutId);
            SetThreadKeyboardLayout(direction == SwitchDirection.Forward ? HklNext : HklPrev);

            var layout = LoadKeyboardLayout(this.GetCurrentLayoutName(), KlfActivate);

            PostMessage(foregroundWindowHandle, WmInputLangChangeRequest, IntPtr.Zero, layout);
        }

        public List<KeyboardLayout> GetKeyboardLayouts()
        {
            this.logger.LogTrace("Getting the list of keyboard layouts in the system");

            int count = GetKeyboardLayoutList(0, null);
            var keyboardLayoutIds = new IntPtr[count];

            GetKeyboardLayoutList(keyboardLayoutIds.Length, keyboardLayoutIds);

            return keyboardLayoutIds
                .Select(keyboardLayoutId => this.CreateKeyboardLayout((int)keyboardLayoutId))
                .ToList();
        }

        private KeyboardLayout GetThreadKeyboardLayout(int threadId)
            => this.CreateKeyboardLayout(GetKeyboardLayout(threadId));

        private void SetThreadKeyboardLayout(int keyboardLayoutId)
            => ActivateKeyboardLayout(keyboardLayoutId, 0);

        private KeyboardLayout CreateKeyboardLayout(int keyboardLayoutId)
            => new KeyboardLayout(
                keyboardLayoutId,
                CultureInfo.GetCultureInfo(keyboardLayoutId & 0xFFFF),
                this.GetLayoutDisplayName(keyboardLayoutId));

        private string GetLayoutDisplayName(int keyboardLayoutId)
        {
            SetThreadKeyboardLayout(keyboardLayoutId);
            string name = this.GetCurrentLayoutName();

            using var key = Registry.LocalMachine.OpenSubKey(String.Format(KeyboardLayoutNameRegistryKeyFormat, name));

            return key?.GetValue(LayoutText)?.ToString() ?? String.Empty;
        }

        private string GetCurrentLayoutName()
        {
            StringBuilder name = new StringBuilder(KlNameLength);
            GetKeyboardLayoutName(name);
            return name.ToString();
        }
    }
}

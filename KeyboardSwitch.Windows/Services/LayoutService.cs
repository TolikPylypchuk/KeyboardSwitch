using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using static KeyboardSwitch.Windows.Interop.Native;

namespace KeyboardSwitch.Windows.Services
{
    public sealed class LayoutService : ILayoutService
    {
        private const string KeyboardLayoutNameRegistryKeyFormat = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts\{0}";
        private const string LayoutText = "Layout Text";

        private readonly ILogger<LayoutService> logger;

        private List<KeyboardLayout>? systemLayouts;

        public LayoutService(ILogger<LayoutService> logger)
            => this.logger = logger;

        public KeyboardLayout GetCurrentKeyboardLayout()
        {
            this.logger.LogDebug("Getting the keyboard layout of the foreground process");
            return this.GetThreadKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));
        }

        public void SwitchCurrentLayout(SwitchDirection direction)
        {
            this.logger.LogDebug($"Switching the keyboard layout of the foregound process {direction.AsString()}");

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
            if (this.systemLayouts != null)
            {
                return this.systemLayouts;
            }

            this.logger.LogDebug("Getting the list of keyboard layouts in the system");

            int count = GetKeyboardLayoutList(0, null);
            var keyboardLayoutIds = new IntPtr[count];

            GetKeyboardLayoutList(keyboardLayoutIds.Length, keyboardLayoutIds);

            this.systemLayouts = keyboardLayoutIds
                .Select(keyboardLayoutId => this.CreateKeyboardLayout((int)keyboardLayoutId))
                .ToList();

            return this.systemLayouts;
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

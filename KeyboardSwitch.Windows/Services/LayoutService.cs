using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using static Vanara.PInvoke.User32;

namespace KeyboardSwitch.Windows.Services
{
    public sealed class LayoutService : ILayoutService
    {
        private const string KeyboardLayoutNameRegistryKeyFormat =
            @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts\{0}";
        private const string LayoutText = "Layout Text";

        private static readonly IntPtr HklNext = (IntPtr)1;
        private static readonly IntPtr HklPrev = (IntPtr)0;
        public const int KlNameLength = 9;

        private readonly ILogger<LayoutService> logger;

        private List<KeyboardLayout>? systemLayouts;

        public LayoutService(ILogger<LayoutService> logger)
            => this.logger = logger;

        public KeyboardLayout GetCurrentKeyboardLayout()
        {
            this.logger.LogDebug("Getting the keyboard layout of the foreground process");
            uint id = GetWindowThreadProcessId(GetForegroundWindow(), out _);
            return this.GetThreadKeyboardLayout(id);
        }

        public void SwitchCurrentLayout(SwitchDirection direction)
        {
            this.logger.LogDebug($"Switching the keyboard layout of the foregound process {direction.AsString()}");

            var foregroundWindowHandle = GetForegroundWindow();
            GetWindowThreadProcessId(foregroundWindowHandle, out uint foregroundWindowThreadId);

            var keyboardLayoutId = GetKeyboardLayout(foregroundWindowThreadId);

            SetThreadKeyboardLayout(keyboardLayoutId);
            SetThreadKeyboardLayout(direction == SwitchDirection.Forward ? HklNext : HklPrev);

            var layout = LoadKeyboardLayout(this.GetCurrentLayoutName(), KLF.KLF_ACTIVATE);
            
            PostMessage(
                foregroundWindowHandle,
                (uint)WindowMessage.WM_INPUTLANGCHANGEREQUEST,
                IntPtr.Zero,
                layout.DangerousGetHandle());
        }

        public List<KeyboardLayout> GetKeyboardLayouts()
        {
            if (this.systemLayouts != null)
            {
                return this.systemLayouts;
            }

            this.logger.LogDebug("Getting the list of keyboard layouts in the system");

            int count = GetKeyboardLayoutList(0, null);
            var keyboardLayoutIds = new HKL[count];

            GetKeyboardLayoutList(keyboardLayoutIds.Length, keyboardLayoutIds);

            this.systemLayouts = keyboardLayoutIds
                .Select(keyboardLayoutId => this.CreateKeyboardLayout(keyboardLayoutId))
                .ToList();

            return this.systemLayouts;
        }

        private KeyboardLayout GetThreadKeyboardLayout(uint threadId)
            => this.CreateKeyboardLayout(GetKeyboardLayout(threadId));

        private void SetThreadKeyboardLayout(HKL keyboardLayoutId)
            => ActivateKeyboardLayout(keyboardLayoutId, 0);

        private KeyboardLayout CreateKeyboardLayout(HKL keyboardLayoutId)
        {
            int id = (int)keyboardLayoutId.DangerousGetHandle();
            return new KeyboardLayout(
                id,
                CultureInfo.GetCultureInfo(id & 0xFFFF),
                this.GetLayoutDisplayName(keyboardLayoutId));
        }

        private string GetLayoutDisplayName(HKL keyboardLayoutId)
        {
            var currentLayout = GetKeyboardLayout(0);

            SetThreadKeyboardLayout(keyboardLayoutId);
            string name = this.GetCurrentLayoutName();

            SetThreadKeyboardLayout(currentLayout);

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

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
    public sealed class LayoutService : ILayoutService, ILayoutLoaderSrevice
    {
        private const string KeyboardLayoutsRegistryKey = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";
        private static readonly string KeyboardLayoutNameRegistryKeyFormat = $@"{KeyboardLayoutsRegistryKey}\{0}";
        private const string LayoutText = "Layout Text";

        private static readonly IntPtr HklNext = (IntPtr)1;
        private static readonly IntPtr HklPrev = (IntPtr)0;
        public const int KlNameLength = 9;

        private readonly ILogger<LayoutService> logger;

        private List<KeyboardLayout>? systemLayouts;

        public LayoutService(ILogger<LayoutService> logger)
            => this.logger = logger;

        public bool CanGetAllSystemLayouts
            => true;

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

        public Dictionary<string, string> GetAllSystemLayouts()
        {
            var layouts = Registry.LocalMachine.OpenSubKey(KeyboardLayoutsRegistryKey);

            return layouts
                .GetSubKeyNames()
                .ToDictionary(
                    layoutKey => layoutKey,
                    layoutKey => layouts.OpenSubKey(layoutKey).GetValue(LayoutText).ToString() ?? String.Empty);
        }

        public DisposableLayouts LoadLayouts(Dictionary<string, string> layouts)
        {
            var loadedLayouts = this.GetKeyboardLayouts();

            var allLayouts = layouts
                .Where(layout => !loadedLayouts.Any(loadedLayout => loadedLayout.Tag == layout.Key))
                .Select(layout =>
                {
                    int id = (int)LoadKeyboardLayout(layout.Key, KLF.KLF_NOTELLSHELL).DangerousGetHandle();
                    return new KeyboardLayout(id, this.GetCultureInfo(id), layout.Value, layout.Key);
                })
                .Concat(loadedLayouts);

            return new UnloadableLayouts(allLayouts, loadedLayouts);
        }

        private KeyboardLayout GetThreadKeyboardLayout(uint threadId)
            => this.CreateKeyboardLayout(GetKeyboardLayout(threadId));

        private void SetThreadKeyboardLayout(HKL keyboardLayoutId)
            => ActivateKeyboardLayout(keyboardLayoutId, 0);

        private KeyboardLayout CreateKeyboardLayout(HKL keyboardLayoutId)
        {
            int id = (int)keyboardLayoutId.DangerousGetHandle();
            var (name, tag) = this.GetLayoutDisplayNameAndTag(keyboardLayoutId);

            return new KeyboardLayout(id, this.GetCultureInfo(id), name, tag);
        }

        private (string DisplayName, string Tag) GetLayoutDisplayNameAndTag(HKL keyboardLayoutId)
        {
            var currentLayout = GetKeyboardLayout(0);

            SetThreadKeyboardLayout(keyboardLayoutId);
            string name = this.GetCurrentLayoutName();

            SetThreadKeyboardLayout(currentLayout);

            using var key = Registry.LocalMachine.OpenSubKey(String.Format(KeyboardLayoutNameRegistryKeyFormat, name));

            return (key?.GetValue(LayoutText)?.ToString() ?? String.Empty, name);
        }

        private string GetCurrentLayoutName()
        {
            StringBuilder name = new StringBuilder(KlNameLength);
            GetKeyboardLayoutName(name);
            return name.ToString();
        }

        private CultureInfo GetCultureInfo(int keyboardLayoutId)
            => CultureInfo.GetCultureInfo(keyboardLayoutId & 0xFFFF);
    }
}

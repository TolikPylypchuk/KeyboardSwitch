using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using static Vanara.PInvoke.User32;

using GC = System.GC;

namespace KeyboardSwitch.Windows.Services
{
    public sealed class LayoutService : ILayoutService, ILayoutLoaderSrevice
    {
        private const string KeyboardLayoutsRegistryKey = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";
        private const string KeyboardLayoutNameRegistryKeyFormat = KeyboardLayoutsRegistryKey + @"\{0}";
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
            uint foregroundWindowThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out _);
            return this.GetThreadKeyboardLayout(foregroundWindowThreadId);
        }

        public void SwitchCurrentLayout(SwitchDirection direction)
        {
            this.logger.LogDebug($"Switching the keyboard layout of the foregound process {direction.AsString()}");

            var foregroundWindowHandle = GetForegroundWindow();
            uint foregroundWindowThreadId = GetWindowThreadProcessId(foregroundWindowHandle, out uint _);

            var keyboardLayoutId = GetKeyboardLayout(foregroundWindowThreadId);

            SetThreadKeyboardLayout(keyboardLayoutId);
            SetThreadKeyboardLayout(direction == SwitchDirection.Forward ? HklNext : HklPrev);

            bool success = PostMessage(
                foregroundWindowHandle,
                (uint)WindowMessage.WM_INPUTLANGCHANGEREQUEST,
                IntPtr.Zero,
                GetKeyboardLayout(0).DangerousGetHandle());

            if (success)
            {
                this.logger.LogDebug("Posted the input lang change message to the foreground window");
            } else
            {
                this.logger.LogError("Failed to post the input lang change message to the foreground window");
            }
        }

        public List<KeyboardLayout> GetKeyboardLayouts()
        {
            if (this.systemLayouts != null)
            {
                return this.systemLayouts;
            }

            this.logger.LogDebug("Getting the list of installed keyboard layouts");

            int count = GetKeyboardLayoutList(0, null);
            var keyboardLayoutIds = new HKL[count];

            int result = GetKeyboardLayoutList(keyboardLayoutIds.Length, keyboardLayoutIds);

            if (result == 0)
            {
                this.logger.LogCritical($"Could not get the list of installed keyboard layouts");
                throw new Win32Exception(result);
            }

            this.systemLayouts = keyboardLayoutIds
                .Select(keyboardLayoutId => this.CreateKeyboardLayout(keyboardLayoutId))
                .ToList();

            return this.systemLayouts;
        }

        public List<LoadableKeyboardLayout> GetAllSystemLayouts()
        {
            this.logger.LogDebug("Getting the list of all keyboard layouts in the system");

            using var layouts = Registry.LocalMachine.OpenSubKey(KeyboardLayoutsRegistryKey);

            return layouts
                .GetSubKeyNames()
                .Select(layoutKey =>
                {
                    using var subKey = layouts.OpenSubKey(layoutKey);
                    return new LoadableKeyboardLayout(
                        layoutKey,
                        subKey.GetValue(LayoutText).ToString() ?? String.Empty);
                })
                .ToList();
        }

        public DisposableLayouts LoadLayouts(IEnumerable<LoadableKeyboardLayout> loadableLayouts)
        {
            this.logger.LogDebug("Loading additional keyboard layouts");

            var loadedLayouts = this.GetKeyboardLayouts();

            var allLayouts = loadableLayouts
                .Select(loadableLayout =>
                {
                    var loadedLayout = loadedLayouts.FirstOrDefault(layout => layout.Tag == loadableLayout.Tag);
                    if (loadedLayout != null)
                    {
                        return loadedLayout;
                    }

                    int id = (int)LoadKeyboardLayout(loadableLayout.Tag, KLF.KLF_NOTELLSHELL).DangerousGetHandle();
                    this.logger.LogInformation($"Loaded layout: {loadableLayout.Name}");

                    return new KeyboardLayout(
                        id, this.GetCultureInfo(id, loadableLayout.Name), loadableLayout.Name, loadableLayout.Tag);
                })
                .ToList();

            return new UnloadableLayouts(allLayouts, loadedLayouts, this.logger);
        }

        private KeyboardLayout GetThreadKeyboardLayout(uint threadId)
            => this.CreateKeyboardLayout(GetKeyboardLayout(threadId));

        private void SetThreadKeyboardLayout(HKL keyboardLayoutId)
            => ActivateKeyboardLayout(keyboardLayoutId, 0);

        private KeyboardLayout CreateKeyboardLayout(HKL keyboardLayoutId)
        {
            int id = (int)keyboardLayoutId.DangerousGetHandle();
            var (name, tag) = this.GetLayoutDisplayNameAndTag(keyboardLayoutId);

            return new KeyboardLayout(id, this.GetCultureInfo(id, name), name, tag);
        }

        private (string DisplayName, string Tag) GetLayoutDisplayNameAndTag(HKL keyboardLayoutId)
        {
            var currentLayout = GetKeyboardLayout(0);

            SetThreadKeyboardLayout(keyboardLayoutId);
            string name = this.GetCurrentLayoutName();

            SetThreadKeyboardLayout(currentLayout);

            using var key = Registry.LocalMachine.OpenSubKey(String.Format(
                CultureInfo.InvariantCulture, KeyboardLayoutNameRegistryKeyFormat, name));

            return (key?.GetValue(LayoutText)?.ToString() ?? String.Empty, name);
        }

        private string GetCurrentLayoutName()
        {
            StringBuilder name = new StringBuilder(KlNameLength);
            GetKeyboardLayoutName(name);
            return name.ToString();
        }

        private CultureInfo GetCultureInfo(int keyboardLayoutId, string layoutName)
        {
            int lcid = keyboardLayoutId & 0xFFFF;

            try
            {
                return CultureInfo.GetCultureInfo(lcid);
            } catch (CultureNotFoundException e)
            {
                this.logger.LogError(e, $"Did not find culture for layout: {layoutName} (LCID {lcid})");
                return CultureInfo.InvariantCulture;
            }
        }
    }
}

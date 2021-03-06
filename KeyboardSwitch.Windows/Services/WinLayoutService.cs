using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using KeyboardSwitch.Core;
using KeyboardSwitch.Core.Keyboard;
using KeyboardSwitch.Core.Services.Layout;
using KeyboardSwitch.Core.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

using static Vanara.PInvoke.User32;

namespace KeyboardSwitch.Windows.Services
{
    internal sealed class WinLayoutService : ILayoutService, ILayoutLoaderSrevice
    {
        private const string KeyboardLayoutsRegistryKey = @"SYSTEM\CurrentControlSet\Control\Keyboard Layouts";
        private const string KeyboardLayoutNameRegistryKeyFormat = KeyboardLayoutsRegistryKey + @"\{0}";
        private const string LayoutText = "Layout Text";

        private static readonly IntPtr HklNext = (IntPtr)1;
        private static readonly IntPtr HklPrev = (IntPtr)0;
        public const int KlNameLength = 9;

        private readonly ILogger<WinLayoutService> logger;

        private List<KeyboardLayout>? systemLayouts;

        public WinLayoutService(ILogger<WinLayoutService> logger) =>
            this.logger = logger;

        public bool SwitchLayoutsViaKeyboardSimulation => false;

        public bool IsLoadingLayoutsSupported => true;

        public KeyboardLayout GetCurrentKeyboardLayout()
        {
            this.logger.LogDebug("Getting the keyboard layout of the foreground process");
            uint foregroundWindowThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out _);
            return this.GetThreadKeyboardLayout(foregroundWindowThreadId);
        }

        public void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
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
                ?.GetSubKeyNames()
                .Select(layoutKey =>
                {
                    using var subKey = layouts.OpenSubKey(layoutKey);
                    return new LoadableKeyboardLayout(
                        layoutKey,
                        subKey?.GetValue(LayoutText)?.ToString() ?? String.Empty);
                })
                .ToList()
                ?? new();
        }

        public DisposableLayouts LoadLayouts(IEnumerable<LoadableKeyboardLayout> loadableLayouts)
        {
            this.logger.LogDebug("Loading additional keyboard layouts");

            var loadedLayouts = this.GetKeyboardLayouts();
            var unloadDisposable = new CompositeDisposable();
            var allLayouts = new List<KeyboardLayout>();

            foreach (var loadableLayout in loadableLayouts)
            {
                var loadedLayout = loadedLayouts.FirstOrDefault(layout => layout.Tag == loadableLayout.Tag);
                if (loadedLayout != null)
                {
                    allLayouts.Add(loadedLayout);
                    continue;
                }

                var layout = LoadKeyboardLayout(loadableLayout.Tag, KLF.KLF_NOTELLSHELL);
                unloadDisposable.Add(layout);

                this.logger.LogDebug($"Loaded layout: {loadableLayout.Name}");

                int id = (int)layout.DangerousGetHandle();
                var culture = this.GetCultureInfo(id, loadableLayout.Name);

                allLayouts.Add(new(id.ToString(), culture.EnglishName, loadableLayout.Name, loadableLayout.Tag));
            }

            return new(allLayouts, unloadDisposable);
        }

        private KeyboardLayout GetThreadKeyboardLayout(uint threadId) =>
            this.CreateKeyboardLayout(GetKeyboardLayout(threadId));

        private void SetThreadKeyboardLayout(HKL keyboardLayoutId) =>
            ActivateKeyboardLayout(keyboardLayoutId, 0);

        private KeyboardLayout CreateKeyboardLayout(HKL keyboardLayoutId)
        {
            int id = (int)keyboardLayoutId.DangerousGetHandle();
            var (name, tag) = this.GetLayoutDisplayNameAndTag(keyboardLayoutId);

            return new(id.ToString(), this.GetCultureInfo(id, name).EnglishName, name, tag);
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
            var name = new StringBuilder(KlNameLength);
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

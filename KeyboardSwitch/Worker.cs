using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyboardSwitch
{
    public class Worker : BackgroundService
    {
        private readonly IKeyboardHookService keyboardHookService;
        private readonly ISwitchService switchService;
        private readonly IAppSettingsService settingsService;
        private readonly IKeysService keysService;
        private readonly ILogger<Worker> logger;

        private IDisposable? hookSubscription;

        public Worker(
            IKeyboardHookService keyboardHookService,
            ISwitchService switchService,
            IAppSettingsService settingsService,
            IKeysService keysService,
            ILogger<Worker> logger)
        {
            this.keyboardHookService = keyboardHookService;
            this.switchService = switchService;
            this.settingsService = settingsService;
            this.keysService = keysService;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            this.logger.LogDebug("Configuring the keyboard switch service");

            await this.RegisterHotKeysAsync();

            this.settingsService.SettingsInvalidated.SubscribeAsync(this.RefreshHotKeysAsync);

            this.logger.LogDebug("Starting the service execution");

            await this.keyboardHookService.WaitForMessagesAsync(token);
        }

        public override void Dispose()
        {
            this.hookSubscription?.Dispose();
            base.Dispose();
        }

        private async Task RegisterHotKeysAsync()
        {
            this.logger.LogDebug("Registering hot keys to switch forward and backward");
            this.RegisterHotKeys(await this.settingsService.GetAppSettingsAsync());
        }

        private async Task RefreshHotKeysAsync()
        {
            this.logger.LogDebug("Refreshing the hot key registration to switch forward and backward");
            this.keyboardHookService.UnregisterAll();
            this.hookSubscription?.Dispose();
            this.RegisterHotKeys(await this.settingsService.GetAppSettingsAsync());
        }

        private void RegisterHotKeys(AppSettings settings)
        {
            switch (settings.SwitchMode)
            {
                case SwitchMode.HotKey when settings.HotKeySwitchSettings != null:
                    this.RegisterHotKeys(settings.HotKeySwitchSettings);
                    break;
                case SwitchMode.ModifierKey when settings.ModifierKeysSwitchSettings != null:
                    this.RegisterHotModifierKeys(settings.ModifierKeysSwitchSettings);
                    break;
                default:
                    throw new InvalidOperationException("Switch settings are invalid");
            }
        }

        private void RegisterHotKeys(HotKeySwitchSettings settings)
        {
            int forwardKeyCode = this.keysService.GetVirtualKeyCode(settings.Forward);
            int backwardKeyCode = this.keysService.GetVirtualKeyCode(settings.Backward);

            this.keyboardHookService.RegisterHotKey(settings.ModifierKeys, forwardKeyCode);
            this.keyboardHookService.RegisterHotKey(settings.ModifierKeys, backwardKeyCode);

            this.hookSubscription = this.keyboardHookService.HotKeyPressed
                .Select(hotKey => hotKey.VirtualKeyCode == forwardKeyCode
                    ? SwitchDirection.Forward
                    : SwitchDirection.Backward)
                .SubscribeAsync(this.switchService.SwitchTextAsync);
        }

        private void RegisterHotModifierKeys(ModifierKeysSwitchSettings settings)
        {
            this.keyboardHookService.RegisterHotModifierKey(
                settings.ForwardModifierKeys, settings.PressCount, settings.WaitMilliseconds);
            this.keyboardHookService.RegisterHotModifierKey(
                settings.BackwardModifierKeys, settings.PressCount, settings.WaitMilliseconds);

            this.hookSubscription = this.keyboardHookService.HotKeyPressed
                .Select(hotKey => hotKey.Modifiers == settings.ForwardModifierKeys
                    ? SwitchDirection.Forward
                    : SwitchDirection.Backward)
                .SubscribeAsync(this.switchService.SwitchTextAsync);
        }
    }
}

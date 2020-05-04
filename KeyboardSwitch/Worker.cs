using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;
using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyboardSwitch
{
    public class Worker : BackgroundService
    {
        private readonly IKeyboardHookService keyboardHookService;
        private readonly ISwitchService switchService;
        private readonly ISettingsService settingsService;
        private readonly IKeysService keysService;
        private readonly INamedPipeService namedPipeService;
        private readonly ILogger<Worker> logger;

        public Worker(
            IKeyboardHookService keyboardHookService,
            ISwitchService switchService,
            ISettingsService settingsService,
            IKeysService keysService,
            ServiceProvider<INamedPipeService> namedPipeProvider,
            ILogger<Worker> logger)
        {
            this.keyboardHookService = keyboardHookService;
            this.switchService = switchService;
            this.settingsService = settingsService;
            this.keysService = keysService;
            this.namedPipeService = namedPipeProvider(nameof(KeyboardSwitch));
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            this.logger.LogDebug("Configuring the keyboard switch service");

            this.SubscribeToExternalCommands();
            await this.RegisterHotKeysAsync();

            this.logger.LogDebug("Starting the service execution");

            await this.keyboardHookService.WaitForMessagesAsync(token);
        }

        private async Task RegisterHotKeysAsync()
        {
            this.logger.LogDebug("Registering hot keys to switch forward and backward");

            var settings = await this.settingsService.GetAppSettingsAsync();

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

            this.keyboardHookService.HotKeyPressed
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

            this.keyboardHookService.HotKeyPressed
                .Select(hotKey => hotKey.Modifiers == settings.ForwardModifierKeys
                    ? SwitchDirection.Forward
                    : SwitchDirection.Backward)
                .SubscribeAsync(this.switchService.SwitchTextAsync);
        }

        private void SubscribeToExternalCommands()
        {
            namedPipeService.ReceivedString
                .Where(command => command.IsCommand(ExternalCommand.ReloadSettings))
                .Do(_ => logger.LogInformation("Invalidating the settings be external request"))
                .Subscribe(_ => this.settingsService.InvalidateSwitchSettings());

            namedPipeService.ReceivedString
                .Where(command => command.IsUnknownCommand())
                .Subscribe(command => logger.LogWarning($"External request '{command}' is not recognized"));
        }
    }
}

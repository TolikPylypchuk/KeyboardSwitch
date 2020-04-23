using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;

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
            INamedPipeService namedPipeService,
            ILogger<Worker> logger)
        {
            this.keyboardHookService = keyboardHookService;
            this.switchService = switchService;
            this.settingsService = settingsService;
            this.keysService = keysService;
            this.namedPipeService = namedPipeService;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            this.logger.LogTrace("Configuring the keyboard switch service");

            this.SubscribeToExternalCommands();
            await this.RegisterHotKeysAsync();

            this.logger.LogTrace("Starting the service execution");

            await this.keyboardHookService.WaitForMessagesAsync(token);
        }

        private async Task RegisterHotKeysAsync()
        {
            this.logger.LogTrace("Registering hot keys to switch forward and backward");

            var settings = await this.settingsService.GetSwitchSettingsAsync();

            var modifiers = settings.ModifierKeys.Flatten();
            int forwardKeyCode = this.keysService.GetVirtualKeyCode(settings.Forward);
            int backwardKeyCode = this.keysService.GetVirtualKeyCode(settings.Backward);

            this.keyboardHookService.RegisterHotKey(modifiers, forwardKeyCode);
            this.keyboardHookService.RegisterHotKey(modifiers, backwardKeyCode);

            this.keyboardHookService.HotKeyPressed
                .Select(hotKey => hotKey.VirtualKeyCode == forwardKeyCode
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

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeyboardSwitch
{
    public class Worker : BackgroundService
    {
        private readonly IKeyboardHookService keyboardHookService;
        private readonly ISwitchService switchService;
        private readonly IAppSettingsService settingsService;
        private readonly IHost host;
        private readonly GlobalSettings globalSettings;
        private readonly ILogger<Worker> logger;

        private IDisposable? hookSubscription;

        public Worker(
            IKeyboardHookService keyboardHookService,
            ISwitchService switchService,
            IAppSettingsService settingsService,
            IHost host,
            IOptions<GlobalSettings> globalSettings,
            ILogger<Worker> logger)
        {
            this.keyboardHookService = keyboardHookService;
            this.switchService = switchService;
            this.settingsService = settingsService;
            this.host = host;
            this.globalSettings = globalSettings.Value;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                this.logger.LogDebug("Configuring the keyboard switch service");

                await this.RegisterHotKeysAsync();

                this.settingsService.SettingsInvalidated.SubscribeAsync(this.RefreshHotKeysAsync);

                this.logger.LogDebug("Starting the service execution");

                await this.keyboardHookService.StartHook(token);
            } catch (IncompatibleAppVersionException e)
            {
                var settingsPath = Environment.ExpandEnvironmentVariables(this.globalSettings.Path);

                this.logger.LogError(e, $"Incompatible app version found in settings: {e.Version}. " +
                    $"Delete the settings at '{settingsPath}' and let the app recreate a compatible version");

                await this.host.StopAsync(token);
            } catch (Exception e)
            {
                this.logger.LogError(e, "Unknown error");
                await this.host.StopAsync(token);
            }
        }

        public override void Dispose()
        {
            this.hookSubscription?.Dispose();
            base.Dispose();
        }

        private async Task RegisterHotKeysAsync()
        {
            this.logger.LogDebug("Registering hot keys to switch forward and backward");
            var settings = await this.settingsService.GetAppSettingsAsync();
            this.RegisterHotKeys(settings.SwitchSettings);
        }

        private async Task RefreshHotKeysAsync()
        {
            this.logger.LogDebug("Refreshing the hot key registration to switch forward and backward");
            this.keyboardHookService.UnregisterAll();
            this.hookSubscription?.Dispose();
            await this.RegisterHotKeysAsync();
        }

        private void RegisterHotKeys(SwitchSettings settings)
        {
            this.keyboardHookService.Register(
                settings.ForwardModifierKeys, settings.PressCount, settings.WaitMilliseconds);
            this.keyboardHookService.Register(
                settings.BackwardModifierKeys, settings.PressCount, settings.WaitMilliseconds);

            this.hookSubscription = this.keyboardHookService.HotKeyPressed
                .Select(key => key.IsSubsetKeyOf(settings.ForwardModifierKeys.Flatten())
                    ? SwitchDirection.Forward
                    : SwitchDirection.Backward)
                .SubscribeAsync(this.switchService.SwitchTextAsync);
        }
    }
}

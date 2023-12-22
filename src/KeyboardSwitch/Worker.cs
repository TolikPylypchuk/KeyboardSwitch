namespace KeyboardSwitch;

using SharpHook;
using SharpHook.Native;

public class Worker(
    IKeyboardHookService keyboardHookService,
    ISwitchService switchService,
    IAppSettingsService settingsService,
    IExitService exitService,
    IOptions<GlobalSettings> globalSettings,
    ILogger<Worker> logger)
    : BackgroundService
{
    private readonly IKeyboardHookService keyboardHookService = keyboardHookService;
    private readonly ISwitchService switchService = switchService;
    private readonly IAppSettingsService settingsService = settingsService;
    private readonly IExitService exitService = exitService;
    private readonly GlobalSettings globalSettings = globalSettings.Value;
    private readonly ILogger<Worker> logger = logger;

    private IDisposable? hookSubscription;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        try
        {
            this.logger.LogDebug("Configuring the KeyboardSwitch service");

            await this.RegisterHotKeysFromSettings();

            this.settingsService.SettingsInvalidated.SubscribeAsync(this.RefreshHotKeys);

            this.logger.LogDebug("Starting the service execution");

            await this.keyboardHookService.StartHook(token);
        } catch (IncompatibleAppVersionException e)
        {
            var settingsPath = Environment.ExpandEnvironmentVariables(this.globalSettings.SettingsFilePath);

            this.logger.LogCritical(
                e,
                "Incompatible app version found in settings: {Version}. " +
                "Delete the settings at '{SettingsPath}' and let the app recreate a compatible version",
                e.Version,
                settingsPath);

            await this.exitService.Exit(ExitCode.IncompatibleSettingsVersion, token);
        } catch (HookException e) when (e.Result == UioHookResult.ErrorAxApiDisabled)
        {
            this.logger.LogCritical(
                e, "The KeyboardSwitch service cannot start as it doesn't have access to the macOS accessibility API");

            await this.exitService.Exit(ExitCode.MacOSAccessibilityDisabled, token);
        } catch (Exception e)
        {
            this.logger.LogCritical(e, "The KeyboardSwitch service has crashed");
            await this.exitService.Exit(ExitCode.Error, token);
        }
    }

    public override void Dispose()
    {
        this.hookSubscription?.Dispose();
        base.Dispose();
    }

    private async Task RegisterHotKeysFromSettings()
    {
        this.logger.LogDebug("Registering hot keys to switch forward and backward");
        var settings = await this.settingsService.GetAppSettings();
        this.RegisterHotKeys(settings.SwitchSettings);
    }

    private void RegisterHotKeys(SwitchSettings settings)
    {
        this.keyboardHookService.Register(
            settings.ForwardModifiers, settings.PressCount, settings.WaitMilliseconds);
        this.keyboardHookService.Register(
            settings.BackwardModifiers, settings.PressCount, settings.WaitMilliseconds);

        this.hookSubscription = this.keyboardHookService.HotKeyPressed
            .Select(key => key.IsSubsetKeyOf(settings.ForwardModifiers.Flatten())
                ? SwitchDirection.Forward
                : SwitchDirection.Backward)
            .SubscribeAsync(this.SwitchText);
    }

    private async Task RefreshHotKeys()
    {
        this.logger.LogDebug("Refreshing the hot key registration to switch forward and backward");
        this.keyboardHookService.UnregisterAll();
        this.hookSubscription?.Dispose();
        await this.RegisterHotKeysFromSettings();
    }

    private async Task SwitchText(SwitchDirection direction)
    {
        try
        {
            await this.switchService.SwitchTextAsync(direction);
        } catch (Exception e)
        {
            this.logger.LogError(e, "Error when trying to switch text");
        }
    }
}

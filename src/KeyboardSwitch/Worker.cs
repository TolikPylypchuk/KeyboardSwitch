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
    private IDisposable? hookSubscription;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        try
        {
            logger.LogDebug("Configuring the KeyboardSwitch service");

            await this.RegisterHotKeysFromSettings();

            settingsService.SettingsInvalidated.SubscribeAsync(this.RefreshHotKeys);

            logger.LogDebug("Starting the service execution");

            await keyboardHookService.StartHook(token);
        } catch (IncompatibleAppVersionException e)
        {
            var settingsPath = Environment.ExpandEnvironmentVariables(globalSettings.Value.SettingsFilePath);

            logger.LogCritical(
                e,
                "Incompatible app version found in settings: {Version}. " +
                "Delete the settings at '{SettingsPath}' and let the app recreate a compatible version",
                e.Version,
                settingsPath);

            await exitService.Exit(ExitCode.IncompatibleSettingsVersion, token);
        } catch (HookException e) when (e.Result == UioHookResult.ErrorAxApiDisabled)
        {
            logger.LogCritical(
                e, "The KeyboardSwitch service cannot start as it doesn't have access to the macOS accessibility API");

            await exitService.Exit(ExitCode.MacOSAccessibilityDisabled, token);
        } catch (Exception e)
        {
            logger.LogCritical(e, "The KeyboardSwitch service has crashed");
            await exitService.Exit(ExitCode.Error, token);
        }
    }

    public override void Dispose()
    {
        this.hookSubscription?.Dispose();
        base.Dispose();
    }

    private async Task RegisterHotKeysFromSettings()
    {
        logger.LogDebug("Registering hot keys to switch forward and backward");
        var settings = await settingsService.GetAppSettings();
        this.RegisterHotKeys(settings.SwitchSettings);
    }

    private void RegisterHotKeys(SwitchSettings settings)
    {
        keyboardHookService.Register(settings.ForwardModifiers, settings.PressCount, settings.WaitMilliseconds);
        keyboardHookService.Register(settings.BackwardModifiers, settings.PressCount, settings.WaitMilliseconds);

        this.hookSubscription = keyboardHookService.HotKeyPressed
            .Select(key => key.IsSubsetKeyOf(settings.ForwardModifiers.Flatten())
                ? SwitchDirection.Forward
                : SwitchDirection.Backward)
            .SubscribeAsync(this.SwitchText);
    }

    private async Task RefreshHotKeys()
    {
        logger.LogDebug("Refreshing the hot key registration to switch forward and backward");
        keyboardHookService.UnregisterAll();
        this.hookSubscription?.Dispose();
        await this.RegisterHotKeysFromSettings();
    }

    private async Task SwitchText(SwitchDirection direction)
    {
        try
        {
            await switchService.SwitchTextAsync(direction);
        } catch (Exception e)
        {
            logger.LogError(e, "Error when trying to switch text");
        }
    }
}

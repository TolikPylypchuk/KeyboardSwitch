namespace KeyboardSwitch;

using SharpHook;
using SharpHook.Native;

public class Worker(
    IKeyboardHookService keyboardHookService,
    ISwitchService switchService,
    IAppSettingsService settingsService,
    IMainLoopRunner mainLoopRunner,
    IExitCodeSetter exitCodeSetter,
    IHost host,
    IOptions<GlobalSettings> globalSettings,
    ILogger<Worker> logger)
    : BackgroundService
{
    private readonly IKeyboardHookService keyboardHookService = keyboardHookService;
    private readonly ISwitchService switchService = switchService;
    private readonly IAppSettingsService settingsService = settingsService;
    private readonly IMainLoopRunner mainLoopRunner = mainLoopRunner;
    private readonly IExitCodeSetter exitCodeSetter = exitCodeSetter;
    private readonly IHost host = host;
    private readonly GlobalSettings globalSettings = globalSettings.Value;
    private readonly ILogger<Worker> logger = logger;

    private IDisposable? hookSubscription;

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        try
        {
            var task = Task.Run(() => this.StartServiceAsync(token), token);

            this.mainLoopRunner.RunMainLoopIfNeeded(token);

            await task;
        } catch (Exception e)
        {
            this.logger.LogCritical(e, "The KeyboardSwitch service has crashed");
            this.exitCodeSetter.AppExitCode = ExitCode.Error;
            await this.host.StopAsync(token);
        }
    }

    public override void Dispose()
    {
        this.hookSubscription?.Dispose();
        base.Dispose();
    }

    private async Task StartServiceAsync(CancellationToken token)
    {
        try
        {
            this.logger.LogDebug("Configuring the KeyboardSwitch service");

            await this.RegisterHotKeys();

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

            this.exitCodeSetter.AppExitCode = ExitCode.IncompatibleSettingsVersion;
            await this.host.StopAsync(token);
        } catch (HookException e) when (e.Result == UioHookResult.ErrorAxApiDisabled)
        {
            this.logger.LogCritical(
                e, "The KeyboardSwitch service cannot start as it doesn't have access to the macOS accessibility API");

            this.exitCodeSetter.AppExitCode = ExitCode.MacOSAccessibilityDisabled;
            await this.host.StopAsync(token);
        } catch (Exception e)
        {
            this.logger.LogCritical(e, "The KeyboardSwitch service has crashed");
            this.exitCodeSetter.AppExitCode = ExitCode.Error;
            await this.host.StopAsync(token);
        }
    }

    private async Task RegisterHotKeys()
    {
        this.logger.LogDebug("Registering hot keys to switch forward and backward");
        var settings = await this.settingsService.GetAppSettingsAsync();
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
        await this.RegisterHotKeys();
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

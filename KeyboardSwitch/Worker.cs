namespace KeyboardSwitch;

public class Worker : BackgroundService
{
    private readonly IRetryManager retryManager;
    private readonly IKeyboardHookService keyboardHookService;
    private readonly ISwitchService switchService;
    private readonly IAppSettingsService settingsService;
    private readonly IHost host;
    private readonly GlobalSettings globalSettings;
    private readonly ILogger<Worker> logger;

    private IDisposable? hookSubscription;

    public Worker(
        IRetryManager retryManager,
        IKeyboardHookService keyboardHookService,
        ISwitchService switchService,
        IAppSettingsService settingsService,
        IHost host,
        IOptions<GlobalSettings> globalSettings,
        ILogger<Worker> logger)
    {
        this.retryManager = retryManager;
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
            this.logger.LogDebug("Configuring the KeyboardSwitch service");

            await this.RegisterHotKeysAsync();

            this.settingsService.SettingsInvalidated.SubscribeAsync(this.RefreshHotKeysAsync);

            this.logger.LogDebug("Starting the service execution");

            await this.retryManager.DoWithRetrying(() => this.keyboardHookService.StartHook(token));
        } catch (IncompatibleAppVersionException e)
        {
            var settingsPath = Environment.ExpandEnvironmentVariables(this.globalSettings.Path);

            this.logger.LogCritical(
                e,
                "Incompatible app version found in settings: {Version}. " +
                "Delete the settings at '{SettingsPath}' and let the app recreate a compatible version",
                e.Version,
                settingsPath);

            await this.host.StopAsync(token);
        } catch (Exception e)
        {
            this.logger.LogCritical(e, "The KeyboardSwitch service has crashed");
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
            settings.ForwardModifiers, settings.PressCount, settings.WaitMilliseconds);
        this.keyboardHookService.Register(
            settings.BackwardModifiers, settings.PressCount, settings.WaitMilliseconds);

        this.hookSubscription = this.keyboardHookService.HotKeyPressed
            .Select(key => key.IsSubsetKeyOf(settings.ForwardModifiers.Flatten())
                ? SwitchDirection.Forward
                : SwitchDirection.Backward)
            .SubscribeAsync(this.SwitchTextSafeAsync);
    }

    private async Task SwitchTextSafeAsync(SwitchDirection direction)
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

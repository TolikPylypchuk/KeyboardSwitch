namespace KeyboardSwitch.Linux.Services;

public sealed class SystemdStartupService : IStartupService
{
    private readonly string ServiceName;
    private readonly ILogger<SystemdStartupService> logger;

    public SystemdStartupService(IOptions<GlobalSettings> settings, ILogger<SystemdStartupService> logger)
    {
        this.ServiceName = settings.Value.SystemdService;
        this.logger = logger;
    }

    public bool IsStartupConfigured(AppSettings settings)
    {
        this.logger.LogDebug("Checking if the KeyboardSwitch service is configured to run on startup");

        bool isConfigured = Systemd.IsLoaded(this.ServiceName) && Systemd.IsEnabled(this.ServiceName);

        this.logger.LogDebug("KeyboardSwitch is configured to run on startup: {IsConfigured}", isConfigured);

        return isConfigured;
    }

    public void ConfigureStartup(AppSettings settings, bool startup)
    {
        this.logger.LogDebug(
            "Configuring to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");

        if (!Systemd.IsLoaded(this.ServiceName))
        {
            this.logger.LogError(
                "Cannot set startup - KeyboardSwitch is not a systemd service. " +
                "Configuring startup is supported only through systemd");
            return;
        }

        if (startup)
        {
            Systemd.Enable(this.ServiceName);
        } else
        {
            Systemd.Disable(this.ServiceName);
        }

        this.logger.LogDebug(
            "Configured to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");
    }
}

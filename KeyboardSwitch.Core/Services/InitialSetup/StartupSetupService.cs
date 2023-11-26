namespace KeyboardSwitch.Core.Services.InitialSetup;

using KeyboardSwitch.Core.Services.Startup;

public class StartupSetupService(
    IStartupService startupService,
    IOptions<GlobalSettings> globalSettings,
    ILogger<StartupSetupService> logger)
    : OneTimeInitialSetupService(globalSettings)
{
    private readonly IStartupService startupService = startupService;
    private readonly ILogger<StartupSetupService> logger = logger;

    public override void DoInitialSetup()
    {
        this.logger.LogInformation("Setting the Keyboard Switch service to start at login");
        this.startupService.ConfigureStartup(startup: true);
    }
}

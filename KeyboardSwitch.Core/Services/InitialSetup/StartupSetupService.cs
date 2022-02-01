namespace KeyboardSwitch.Core.Services.InitialSetup;

using KeyboardSwitch.Core.Services.Startup;

public class StartupSetupService : OneTimeInitialSetupService
{
    private readonly IStartupService startupService;
    private readonly ILogger<StartupSetupService> logger;

    public StartupSetupService(
        IStartupService startupService,
        IOptions<GlobalSettings> globalSettings,
        ILogger<StartupSetupService> logger)
        : base(globalSettings)
    {
        this.startupService = startupService;
        this.logger = logger;
    }

    public override void DoInitialSetup()
    {
        this.logger.LogInformation("Setting the Keyboard Switch service to start at login");
        this.startupService.ConfigureStartup(startup: true);
    }
}

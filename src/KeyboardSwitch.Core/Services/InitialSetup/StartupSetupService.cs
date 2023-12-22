namespace KeyboardSwitch.Core.Services.InitialSetup;

using KeyboardSwitch.Core.Services.Startup;

public class StartupSetupService(
    IStartupService startupService,
    IOptions<GlobalSettings> globalSettings,
    ILogger<StartupSetupService> logger)
    : OneTimeInitialSetupService(globalSettings)
{
    public override void DoInitialSetup()
    {
        logger.LogInformation("Setting the Keyboard Switch service to start at login");
        startupService.ConfigureStartup(startup: true);
    }
}

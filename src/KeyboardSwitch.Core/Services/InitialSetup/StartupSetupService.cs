namespace KeyboardSwitch.Core.Services.InitialSetup;

using KeyboardSwitch.Core.Services.Startup;
using KeyboardSwitch.Core.Services.Users;

public class StartupSetupService(
    IStartupService startupService,
    IUserProvider userProvider,
    IOptions<GlobalSettings> globalSettings,
    ILogger<StartupSetupService> logger)
    : OneTimeInitialSetupService(userProvider, globalSettings, logger)
{
    protected override void DoInitialSetup(string currentUser)
    {
        logger.LogInformation("Setting the Keyboard Switch service to start at login");
        startupService.ConfigureStartup(startup: true);
    }
}

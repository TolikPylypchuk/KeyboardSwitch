namespace KeyboardSwitch.Core.Services.InitialSetup;

using System.IO.Abstractions;

using KeyboardSwitch.Core.Services.Startup;
using KeyboardSwitch.Core.Services.Users;

public class StartupSetupService(
    IStartupService startupService,
    IUserProvider userProvider,
    IFileSystem fileSystem,
    IOptions<GlobalSettings> globalSettings,
    ILogger<StartupSetupService> logger)
    : OneTimeInitialSetupService(userProvider, fileSystem, globalSettings, logger)
{
    protected override void DoInitialSetup(string currentUser)
    {
        logger.LogInformation("Setting the Keyboard Switch service to start at login");
        startupService.ConfigureStartup(startup: true);
    }
}

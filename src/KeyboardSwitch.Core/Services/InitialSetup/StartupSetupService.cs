using System.IO.Abstractions;

using KeyboardSwitch.Core.Services.Startup;
using KeyboardSwitch.Core.Services.Users;

namespace KeyboardSwitch.Core.Services.InitialSetup;

public partial class StartupSetupService(
    IStartupService startupService,
    IUserProvider userProvider,
    IFileSystem fileSystem,
    IOptions<GlobalSettings> globalSettings,
    ILogger<StartupSetupService> logger)
    : InitialSetupServiceBase(userProvider, fileSystem, globalSettings, logger)
{
    protected override void DoInitialSetup(string currentUser, bool firstTime)
    {
        if (firstTime)
        {
            this.LogSettingStartAtLogin();
            startupService.ConfigureStartup(startup: true);
        }
    }

    [LoggerMessage(LogLevel.Information, "Setting the Keyboard Switch service to start at login")]
    private partial void LogSettingStartAtLogin();
}

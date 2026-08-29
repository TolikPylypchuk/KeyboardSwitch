using System.IO.Abstractions;

namespace KeyboardSwitch.MacOS.Services;

internal sealed partial class LaunchdSetupService(
    IUserProvider userProvider,
    IFileSystem fileSystem,
    IOptions<GlobalSettings> globalSettings,
    IOptions<LaunchdSettings> launchdSettings,
    ILogger<LaunchdSetupService> logger)
    : InitialSetupServiceBase(userProvider, fileSystem, globalSettings, logger)
{
    private readonly string serviceDescriptorPath = launchdSettings.Value.ServiceDescriptorPath;

    protected override void DoInitialSetup(string currentUser, bool firstTime)
    {
        if (firstTime)
        {
            this.LogBootstrappingService();
            Process.Start(LaunchCtl, $"bootstrap gui/{currentUser} {serviceDescriptorPath}");
        }
    }

    [LoggerMessage(LogLevel.Information, "Bootstrapping the Keyboard Switch service for the current user")]
    private partial void LogBootstrappingService();
}

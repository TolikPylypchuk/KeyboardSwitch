namespace KeyboardSwitch.MacOS.Services;

using System.IO.Abstractions;

internal sealed class LaunchdSetupService(
    IUserProvider userProvider,
    IFileSystem fileSystem,
    IOptions<GlobalSettings> globalSettings,
    IOptions<LaunchdSettings> launchdSettings,
    ILogger<LaunchdSetupService> logger)
    : OneTimeInitialSetupService(userProvider, fileSystem, globalSettings, logger)
{
    private readonly string serviceDescriptorPath = launchdSettings.Value.ServiceDescriptorPath;

    protected override void DoInitialSetup(string currentUser)
    {
        logger.LogInformation("Bootstrapping the Keyboard Switch service for the current user");
        Process.Start(LaunchCtl, $"bootstrap gui/{currentUser} {serviceDescriptorPath}");
    }
}

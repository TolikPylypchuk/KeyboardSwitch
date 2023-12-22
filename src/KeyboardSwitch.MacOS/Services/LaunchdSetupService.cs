namespace KeyboardSwitch.MacOS.Services;

internal sealed class LaunchdSetupService(
    IOptions<GlobalSettings> globalSettings,
    IOptions<LaunchdSettings> launchdSettings,
    ILogger<LaunchdSetupService> logger)
    : OneTimeInitialSetupService(globalSettings)
{
    private readonly string serviceDescriptorPath = launchdSettings.Value.ServiceDescriptorPath;

    public override void DoInitialSetup()
    {
        int? id = GetCurrentUserId();

        if (id != null)
        {
            logger.LogInformation("Bootstrapping the Keyboard Switch service for the current user");

            Process.Start(LaunchCtl, $"bootstrap gui/{id} {serviceDescriptorPath}");
        } else
        {
            logger.LogError("Could not bootstrap the Keyboard Switch service - couldn't find the current user's ID");
        }
    }
}

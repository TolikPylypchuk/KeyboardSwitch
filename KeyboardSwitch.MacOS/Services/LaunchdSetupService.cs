namespace KeyboardSwitch.MacOS.Services;

internal sealed class LaunchdSetupService : OneTimeInitialSetupService
{
    private readonly string serviceDescriptorPath;
    private readonly ILogger<LaunchdSetupService> logger;

    public LaunchdSetupService(
        IOptions<GlobalSettings> globalSettings,
        IOptions<LaunchdSettings> launchdSettings,
        ILogger<LaunchdSetupService> logger)
        : base(globalSettings)
    {
        this.serviceDescriptorPath = launchdSettings.Value.ServiceDescriptorPath;
        this.logger = logger;
    }

    public override void DoInitialSetup()
    {
        int? id = GetCurrentUserId();

        if (id != null)
        {
            this.logger.LogInformation("Bootstrapping the Keyboard Switch service for the current user");

            Process.Start(LaunchCtl, $"bootstrap gui/{id} {serviceDescriptorPath}");
        } else
        {
            this.logger.LogError(
                "Could not bootstrap the Keyboard Switch service - " +
                "couldn't find the current user's ID");
        }
    }
}

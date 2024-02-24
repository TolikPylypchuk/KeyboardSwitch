namespace KeyboardSwitch.MacOS.Services;

internal sealed class LaunchdStartupService(
    IUserProvider userProvider,
    IOptions<LaunchdSettings> launchdSettings,
    ILogger<LaunchdStartupService> logger)
    : IStartupService
{
    private readonly string serivceName = launchdSettings.Value.ServiceName;

    public bool IsStartupConfigured()
    {
        logger.LogDebug("Checking if the Keyboard Switch service is configured to run on startup");

        string? user = userProvider.GetCurrentUser();

        if (!String.IsNullOrEmpty(user))
        {
            var launchctl = Process.Start(
                new ProcessStartInfo(LaunchCtl, $"print gui/{user}") { RedirectStandardOutput = true });

            if (launchctl is not null)
            {
                string output = launchctl.StandardOutput.ReadToEnd();

                int searchStart = output.IndexOf("disabled services");
                return searchStart == -1 || !output[searchStart..].Contains($"\"{this.serivceName}\" => true");
            }
        } else
        {
            logger.LogError(
                "Could not check whether the Keyboard Switch service is configured to run on startup - " +
                "couldn't find the current user's ID");
        }

        return false;
    }

    public void ConfigureStartup(bool startup)
    {
        logger.LogDebug(
            "Configuring to {Action} running the Keyboard Switch service on startup", startup ? "start" : "stop");

        string? user = userProvider.GetCurrentUser();

        if (!String.IsNullOrEmpty(user))
        {
            Process.Start(LaunchCtl, $"{(startup ? "enable" : "disable")} gui/{user}/{this.serivceName}");

            logger.LogDebug(
                "Configured to {Action} running the Keyboard Switch service on startup", startup ? "start" : "stop");
        } else
        {
            logger.LogError(
                "Could not configure to {Action} running the Keyboard Switch service on startup - " +
                "couldn't find the current user's ID",
                startup ? "start" : "stop");
        }
    }
}

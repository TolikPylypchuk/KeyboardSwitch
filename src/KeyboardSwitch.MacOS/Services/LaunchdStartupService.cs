namespace KeyboardSwitch.MacOS.Services;

internal sealed partial class LaunchdStartupService(
    IUserProvider userProvider,
    IOptions<LaunchdSettings> launchdSettings,
    ILogger<LaunchdStartupService> logger)
    : IStartupService
{
    private readonly string serivceName = launchdSettings.Value.ServiceName;

    public bool IsStartupConfigured()
    {
        this.LogCheckingIfServiceConfiguredToRunAtStartup();

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
            this.LogCheckCouldNotFileCurrentUserId();
        }

        return false;
    }

    public void ConfigureStartup(bool startup)
    {
        this.LogConfiguringRunningAtStartup(startup ? "start" : "stop");

        string? user = userProvider.GetCurrentUser();

        if (!String.IsNullOrEmpty(user))
        {
            Process.Start(LaunchCtl, $"{(startup ? "enable" : "disable")} gui/{user}/{this.serivceName}");

            this.LogConfiguredRunningAtStartup(startup ? "start" : "stop");
        } else
        {
            this.LogConfigureCouldNotFileCurrentUserId(startup ? "start" : "stop");
        }
    }

    [LoggerMessage(LogLevel.Debug, "Checking if the Keyboard Switch service is configured to run on startup")]
    private partial void LogCheckingIfServiceConfiguredToRunAtStartup();

    [LoggerMessage(
        LogLevel.Error,
        "Could not check whether the Keyboard Switch service is configured to run on startup - " +
        "couldn't find the current user's ID")]
    private partial void LogCheckCouldNotFileCurrentUserId();

    [LoggerMessage(LogLevel.Debug, "Configuring to {Action} running the Keyboard Switch service on startup")]
    private partial void LogConfiguringRunningAtStartup(string action);

    [LoggerMessage(LogLevel.Debug, "Configured to {Action} running the Keyboard Switch service on startup")]
    private partial void LogConfiguredRunningAtStartup(string action);

    [LoggerMessage(
        LogLevel.Error,
        "Could not configure to {Action} running the Keyboard Switch service on startup - " +
        "couldn't find the current user's ID")]
    private partial void LogConfigureCouldNotFileCurrentUserId(string action);
}

namespace KeyboardSwitch.MacOS.Services;

using System.Diagnostics;

internal class LaunchdStartupService : IStartupService
{
    private const string Launchctl = "launchctl";
    private const string KeyboardSwitchService = "io.tolik.keyboardswitch";

    private readonly ILogger<LaunchdStartupService> logger;

    public LaunchdStartupService(ILogger<LaunchdStartupService> logger) =>
        this.logger = logger;

    public bool IsStartupConfigured()
    {
        this.logger.LogDebug("Checking if the KeyboardSwitch service is configured to run on startup");

        int? id = this.GetCurrentUserId();

        if (id != null)
        {
            var launchctl = Process.Start(
                new ProcessStartInfo(Launchctl, $"print gui/{id}") { RedirectStandardOutput = true });

            if (launchctl != null)
            {
                string output = launchctl.StandardOutput.ReadToEnd();

                int searchStart = output.IndexOf("disabled services");
                return searchStart == -1 || !output[searchStart..].Contains($"\"{KeyboardSwitchService}\" => true");
            }
        } else
        {
            this.logger.LogError(
                "Could not check whether the KeyboardSwitch service is configured to run on startup - " +
                "couldn't find the current user's ID");
        }

        return false;
    }

    public void ConfigureStartup(bool startup)
    {
        this.logger.LogDebug(
            "Configuring to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");

        int? id = GetCurrentUserId();

        if (id != null)
        {
            Process.Start(Launchctl, $"{(startup ? "enable" : "disable")} gui/{id}/{KeyboardSwitchService}");

            this.logger.LogDebug(
                "Configured to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");
        } else
        {
            this.logger.LogError(
                "Could not configure to {Action} running the KeyboardSwitch service on startup - " +
                "couldn't find the current user's ID",
                startup ? "start" : "stop");
        }
    }

    private int? GetCurrentUserId()
    {
        var id = Process.Start(new ProcessStartInfo("id", "-u") { RedirectStandardOutput = true });

        if (id != null)
        {
            string output = id.StandardOutput.ReadToEnd();
            return Int32.TryParse(output.Trim() ?? String.Empty, out int result) ? result : null;
        }

        return null;
    }
}

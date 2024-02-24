namespace KeyboardSwitch.Windows.Services;

using Microsoft.Extensions.Options;

internal class RegistryStartupService(IOptions<GlobalSettings> globalSettings, ILogger<RegistryStartupService> logger)
    : IStartupService
{
    private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupRegistryName = "Keyboard Switch";
    private const string ExecutableExtension = ".exe";

    public bool IsStartupConfigured()
    {
        logger.LogDebug("Checking if the Keyboard Switch service is configured to run on startup");

        using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey);
        bool isConfigured = key?.GetValue(StartupRegistryName) != null;

        logger.LogDebug("Keyboard Switch is configured to run on startup: {IsConfigured}", isConfigured);

        return isConfigured;
    }

    public void ConfigureStartup(bool startup)
    {
        logger.LogDebug(
            "Configuring to {Action} running the Keyboard Switch service on startup", startup ? "start" : "stop");

        using var startupKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);

        if (startup)
        {
            startupKey?.SetValue(StartupRegistryName, this.GetServicePath(), RegistryValueKind.String);
        } else
        {
            startupKey?.DeleteValue(StartupRegistryName);
        }

        logger.LogDebug(
            "Configured to {Action} running the Keyboard Switch service on startup", startup ? "start" : "stop");
    }

    private string GetServicePath()
    {
        var path = globalSettings.Value.ServicePath.EndsWith(
            ExecutableExtension, StringComparison.InvariantCultureIgnoreCase)
            ? globalSettings.Value.ServicePath
            : globalSettings.Value.ServicePath + ExecutableExtension;

        return $"\"{Path.GetFullPath(path)}\"";
    }
}

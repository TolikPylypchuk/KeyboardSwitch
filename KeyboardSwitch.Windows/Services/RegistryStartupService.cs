using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Windows.Services;

internal class RegistryStartupService : IStartupService
{
    private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupRegistryName = "Keyboard Switch";
    private const string ExecutableExtension = ".exe";

    private readonly GlobalSettings globalSettings;
    private readonly ILogger<RegistryStartupService> logger;

    public RegistryStartupService(IOptions<GlobalSettings> globalSettings, ILogger<RegistryStartupService> logger)
    {
        this.globalSettings = globalSettings.Value;
        this.logger = logger;
    }

    public bool IsStartupConfigured()
    {
        this.logger.LogDebug("Checking if the KeyboardSwitch service is configured to run on startup");

        using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey);
        bool isConfigured = key?.GetValue(StartupRegistryName) != null;

        this.logger.LogDebug("KeyboardSwitch is configured to run on startup: {IsConfigured}", isConfigured);

        return isConfigured;
    }

    public void ConfigureStartup(bool startup)
    {
        this.logger.LogDebug(
            "Configuring to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");

        using var startupKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);

        if (startup)
        {
            startupKey?.SetValue(StartupRegistryName, this.GetServicePath(), RegistryValueKind.String);
        } else
        {
            startupKey?.DeleteValue(StartupRegistryName);
        }

        this.logger.LogDebug(
            "Configured to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");
    }

    private string GetServicePath()
    {
        var path = this.globalSettings.ServicePath.EndsWith(
            ExecutableExtension, StringComparison.InvariantCultureIgnoreCase)
            ? this.globalSettings.ServicePath
            : this.globalSettings.ServicePath + ExecutableExtension;

        return $"\"{Path.GetFullPath(path)}\"";
    }
}

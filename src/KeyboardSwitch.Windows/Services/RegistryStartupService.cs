using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Windows.Services;

internal sealed partial class RegistryStartupService(
    IOptions<GlobalSettings> globalSettings,
    ILogger<RegistryStartupService> logger)
    : IStartupService
{
    private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupRegistryName = Constants.ApplicationName;
    private const string ExecutableExtension = ".exe";

    public bool IsStartupConfigured()
    {
        this.LogCheckingServiceConfiguredToRunAtStartup();

        using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey);
        bool isConfigured = key?.GetValue(StartupRegistryName) != null;

        this.LogServiceConfiguredToRunAtStartup(isConfigured);

        return isConfigured;
    }

    public void ConfigureStartup(bool startup)
    {
        this.LogConfiguringServiceConfiguredToRunAtStartup(startup ? "start" : "stop");

        using var startupKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);

        if (startup)
        {
            startupKey?.SetValue(StartupRegistryName, this.GetServicePath(), RegistryValueKind.String);
        } else
        {
            startupKey?.DeleteValue(StartupRegistryName);
        }

        this.LogConfiguredServiceConfiguredToRunAtStartup(startup ? "start" : "stop");
    }

    private string GetServicePath()
    {
        var path = globalSettings.Value.ServicePath.EndsWith(
            ExecutableExtension, StringComparison.InvariantCultureIgnoreCase)
            ? globalSettings.Value.ServicePath
            : globalSettings.Value.ServicePath + ExecutableExtension;

        return $"\"{Path.GetFullPath(path)}\"";
    }

    [LoggerMessage(LogLevel.Debug, "Checking if the Keyboard Switch service is configured to run on startup")]
    private partial void LogCheckingServiceConfiguredToRunAtStartup();

    [LoggerMessage(LogLevel.Debug, "Keyboard Switch is configured to run on startup: {IsConfigured}")]
    private partial void LogServiceConfiguredToRunAtStartup(bool isConfigured);

    [LoggerMessage(LogLevel.Debug, "Configuring to {Action} running the Keyboard Switch service on startup")]
    private partial void LogConfiguringServiceConfiguredToRunAtStartup(string action);

    [LoggerMessage(LogLevel.Debug, "Configured to {Action} running the Keyboard Switch service on startup")]
    private partial void LogConfiguredServiceConfiguredToRunAtStartup(string action);
}

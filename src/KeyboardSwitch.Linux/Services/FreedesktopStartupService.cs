namespace KeyboardSwitch.Linux.Services;

internal sealed partial class FreedesktopStartupService(
    IOptions<GlobalSettings> globalSettings,
    IOptions<StartupSettings> startupSettings,
    ILogger<FreedesktopStartupService> logger)
    : IStartupService
{
    private const string StartFileContent = """
        [Desktop Entry]
        Version=1.0
        Name=Keyboard Switch
        Comment=Switches typed text as if it were typed with another keyboard layout
        Exec=$SERVICE_APP
        TryExec=$SERVICE_APP
        Path=$DIRECTORY
        Icon=$DIRECTORY/keyboard-switch.png
        Terminal=false
        Type=Application
        Categories=Utility
        """;

    private const string AppNamePlaceholder = "$SERVICE_APP";
    private const string AppDirectoryPlaceholder = "$DIRECTORY";

    private readonly string startupFilePath =
        Environment.ExpandEnvironmentVariables(startupSettings.Value.StartupFilePath);

    public bool IsStartupConfigured()
    {
        this.LogCheckingConfigured();

        bool isConfigured = File.Exists(this.startupFilePath);

        this.LogConfiguedToRunOnStartup(isConfigured);

        return isConfigured;
    }

    public void ConfigureStartup(bool startup)
    {
        this.LogConfiguringStartingOnStartup(startup ? "start" : "stop");

        if (startup)
        {
            if (Path.GetDirectoryName(this.startupFilePath) is string directory)
            {
                Directory.CreateDirectory(directory);
            }

            string servicePath = Path.GetFullPath(globalSettings.Value.ServicePath);

            string fileContent = StartFileContent.ReplaceLineEndings()
                .Replace(AppNamePlaceholder, servicePath)
                .Replace(AppDirectoryPlaceholder, Path.GetDirectoryName(servicePath));

            using var writer = new StreamWriter(File.Create(this.startupFilePath));
            writer.Write(fileContent);
        } else
        {
            File.Delete(this.startupFilePath);
        }

        this.LogConfiguredStartingOnStartup(startup ? "start" : "stop");
    }

    [LoggerMessage(LogLevel.Debug, "Checking if the Keyboard Switch service is configured to run on startup")]
    private partial void LogCheckingConfigured();

    [LoggerMessage(LogLevel.Debug, "Keyboard Switch is configured to run on startup: {IsConfigured}")]
    private partial void LogConfiguedToRunOnStartup(bool isConfigured);

    [LoggerMessage(LogLevel.Debug, "Configuring to {Action} running the Keyboard Switch service on startup")]
    private partial void LogConfiguringStartingOnStartup(string action);

    [LoggerMessage(LogLevel.Debug, "Configured to {Action} running the KeyboardSwitch service on startup")]
    private partial void LogConfiguredStartingOnStartup(string action);
}

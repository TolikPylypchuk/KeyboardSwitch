namespace KeyboardSwitch.Linux.Services;

internal sealed class FreedesktopStartupService(
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
        Icon=$DIRECTORY/icon.png
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
        logger.LogDebug("Checking if the KeyboardSwitch service is configured to run on startup");

        bool isConfigured = File.Exists(this.startupFilePath);

        logger.LogDebug("KeyboardSwitch is configured to run on startup: {IsConfigured}", isConfigured);

        return isConfigured;
    }

    public void ConfigureStartup(bool startup)
    {
        logger.LogDebug(
            "Configuring to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");

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

        logger.LogDebug(
            "Configured to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");
    }
}

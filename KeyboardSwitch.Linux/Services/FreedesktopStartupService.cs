namespace KeyboardSwitch.Linux.Services;

internal sealed class FreedesktopStartupService : IStartupService
{
    private const string StartFileContent = @"[Desktop Entry]
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
";

    private const string AppNamePlaceholder = "$SERVICE_APP";
    private const string AppDirectoryPlaceholder = "$DIRECTORY";

    private readonly string startupFilePath;
    private readonly ILogger<FreedesktopStartupService> logger;

    public FreedesktopStartupService(IOptions<StartupSettings> settings, ILogger<FreedesktopStartupService> logger)
    {
        this.startupFilePath = Environment.ExpandEnvironmentVariables(settings.Value.StartupFilePath);
        this.logger = logger;
    }

    public bool IsStartupConfigured(AppSettings settings)
    {
        this.logger.LogDebug("Checking if the KeyboardSwitch service is configured to run on startup");

        bool isConfigured = File.Exists(this.startupFilePath);

        this.logger.LogDebug("KeyboardSwitch is configured to run on startup: {IsConfigured}", isConfigured);

        return isConfigured;
    }

    public void ConfigureStartup(AppSettings settings, bool startup)
    {
        this.logger.LogDebug(
            "Configuring to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");

        if (startup)
        {
            if (Path.GetDirectoryName(this.startupFilePath) is string directory)
            {
                Directory.CreateDirectory(directory);
            }

            string servicePath = Path.GetFullPath(settings.ServicePath);

            using var writer = new StreamWriter(File.Create(this.startupFilePath));
            writer.Write(StartFileContent.ReplaceLineEndings()
                .Replace(AppNamePlaceholder, servicePath)
                .Replace(AppDirectoryPlaceholder, Path.GetDirectoryName(servicePath)));
        } else
        {
            File.Delete(this.startupFilePath);
        }

        this.logger.LogDebug(
            "Configured to {Action} running the KeyboardSwitch service on startup", startup ? "start" : "stop");
    }
}

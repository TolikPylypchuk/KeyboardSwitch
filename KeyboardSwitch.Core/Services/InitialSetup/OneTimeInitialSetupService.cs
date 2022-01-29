namespace KeyboardSwitch.Core.Services.InitialSetup;

public abstract class OneTimeInitialSetupService : IInitialSetupService
{
    private readonly string initialSetupFilePath;

    public OneTimeInitialSetupService(IOptions<GlobalSettings> globalSettings) =>
        this.initialSetupFilePath = Environment.ExpandEnvironmentVariables(globalSettings.Value.InitialSetupFilePath);

    public void InitializeKeyboardSwitchSetup()
    {
        if (this.ShouldDoSetup())
        {
            this.DoInitialSetup();
            File.Create(this.initialSetupFilePath).Dispose();
        }
    }

    public abstract void DoInitialSetup();

    private bool ShouldDoSetup()
    {
        if (!File.Exists(this.initialSetupFilePath))
        {
            return true;
        }

        string? processPath = Environment.ProcessPath;

        return processPath != null &&
            File.GetLastWriteTimeUtc(this.initialSetupFilePath) > File.GetCreationTimeUtc(processPath);
    }
}

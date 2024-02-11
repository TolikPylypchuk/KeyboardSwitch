namespace KeyboardSwitch.Core.Services.InitialSetup;

using System.Text.Json;

using KeyboardSwitch.Core.Json;
using KeyboardSwitch.Core.Services.Users;

public abstract class OneTimeInitialSetupService(
    IUserProvider userProvider,
    IOptions<GlobalSettings> globalSettings,
    ILogger<OneTimeInitialSetupService> logger)
    : IInitialSetupService
{
    private readonly FileInfo initialSetupFile =
        new(Environment.ExpandEnvironmentVariables(globalSettings.Value.InitialSetupFilePath));

    public void InitializeKeyboardSwitchSetup()
    {
        var currentUser = userProvider.GetCurrentUser();

        if (String.IsNullOrEmpty(currentUser))
        {
            logger.LogError("Could not get current user - no initial setup will be done");
            return;
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                this.InitializeKeyboardSwitchSetupForWindows(currentUser, this.DoInitialSetup);
            } else
            {
                this.InitializeKeyboardSwitchSetupForNonWindows(currentUser, this.DoInitialSetup);
            }
        } catch (Exception e)
        {
            logger.LogError(e, "Error during initial setup");
        }
    }

    protected abstract void DoInitialSetup(string currentUser);

    private void InitializeKeyboardSwitchSetupForWindows(string currentUser, Action<string> doSetup)
    {
        var users = this.ReadUsersFromFile();

        if (!users.Contains(currentUser))
        {
            doSetup(currentUser);

            users.Add(currentUser);
            this.WriteToUsersFile(users);
        }
    }

    private List<string> ReadUsersFromFile()
    {
        if (!this.initialSetupFile.Exists)
        {
            return [];
        }

        using var fileStream = new BufferedStream(this.initialSetupFile.OpenRead());
        return JsonSerializer.Deserialize(fileStream, KeyboardSwitchJsonContext.Default.ListString) ?? [];
    }

    private void WriteToUsersFile(List<string> users)
    {
        using var fileStream = new BufferedStream(this.initialSetupFile.OpenWrite());
        JsonSerializer.Serialize(fileStream, users, KeyboardSwitchJsonContext.Default.ListString);
    }

    private void InitializeKeyboardSwitchSetupForNonWindows(string currentUser, Action<string> doSetup)
    {
        bool fileExists = this.initialSetupFile.Exists;

        string? processPath = Environment.ProcessPath;
        bool shouldDoSetup = !fileExists ||
            processPath is not null && this.initialSetupFile.LastWriteTimeUtc < File.GetCreationTimeUtc(processPath);

        if (shouldDoSetup)
        {
            doSetup(currentUser);
        }

        if (!fileExists)
        {
            this.initialSetupFile.Create().Dispose();
        }

        this.initialSetupFile.LastWriteTimeUtc = DateTime.UtcNow;
    }
}

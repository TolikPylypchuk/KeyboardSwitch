using System.IO.Abstractions;
using System.Text.Json;

using KeyboardSwitch.Core.Json;
using KeyboardSwitch.Core.Services.Users;

namespace KeyboardSwitch.Core.Services.InitialSetup;

public abstract class InitialSetupServiceBase(
    IUserProvider userProvider,
    IFileSystem fileSystem,
    IOptions<GlobalSettings> globalSettings,
    ILogger<InitialSetupServiceBase> logger)
    : IInitialSetupService
{
    private readonly IFileInfo initialSetupFile =
        fileSystem.FileInfo.New(Environment.ExpandEnvironmentVariables(globalSettings.Value.InitialSetupFilePath));

    protected readonly IFileSystem FileSystem = fileSystem;

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
                this.InitializeKeyboardSwitchSetupForWindows(currentUser);
            } else
            {
                this.InitializeKeyboardSwitchSetupForNonWindows(currentUser);
            }
        } catch (Exception e)
        {
            logger.LogError(e, "Error during initial setup");
        }
    }

    protected abstract void DoInitialSetup(string currentUser, bool firstTime);

    private void InitializeKeyboardSwitchSetupForWindows(string currentUser)
    {
        var users = this.ReadUsersFromFile();

        if (!users.Contains(currentUser))
        {
            this.DoInitialSetupSafe(currentUser, true);

            users.Add(currentUser);
            this.WriteToUsersFile(users);
        } else
        {
            this.DoInitialSetupSafe(currentUser, false);
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

    private void InitializeKeyboardSwitchSetupForNonWindows(string currentUser)
    {
        bool fileExists = this.initialSetupFile.Exists;

        string? processPath = Environment.ProcessPath;
        bool firstTime = !fileExists ||
            processPath is not null && this.initialSetupFile.LastWriteTimeUtc < File.GetCreationTimeUtc(processPath);

        this.DoInitialSetupSafe(currentUser, firstTime);

        if (!fileExists)
        {
            this.initialSetupFile.Create().Dispose();
        }

        this.initialSetupFile.LastWriteTimeUtc = DateTime.UtcNow;
    }

    private void DoInitialSetupSafe(string currentUser, bool firstTime)
    {
        try
        {
            this.DoInitialSetup(currentUser, firstTime);
        } catch (Exception e)
        {
            logger.LogError(e, "Exception when doing the initial setup");
        }
    }
}

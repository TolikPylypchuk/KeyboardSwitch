using System.IO.Abstractions;

namespace KeyboardSwitch.Linux.Services;

internal sealed class LinuxSetupService(
    IStartupService startupService,
    IUserProvider userProvider,
    IFileSystem fileSystem,
    IOptions<GlobalSettings> globalSettings,
    ILogger<LinuxSetupService> logger)
    : StartupSetupService(startupService, userProvider, fileSystem, globalSettings, logger)
{
    private static readonly string GnomeExtensionsDirectory = "gnome-extension";

    private static readonly string GnomeExtensionV1Directory = "v1";
    private static readonly string GnomeExtensionV2Directory = "v2";

    private static readonly string GnomeExtensionJsFile = "extension.js";
    private static readonly string GnomeExtensionMetadataFile = "metadata.json";

    private static readonly string GlobalGnomeExtensionTargetDirectory =
        "/usr/share/gnome-shell/extensions/switch-layout@tolik.io";

    private static readonly string LocalGnomeExtensionTargetDirectory =
        ".local/share/gnome-shell/extensions/switch-layout@tolik.io";

    private static readonly Version GnomeVersionForExtensionV2 = new(45, 0);

    protected override void DoInitialSetup(string currentUser, bool firstTime)
    {
        base.DoInitialSetup(currentUser, firstTime);

        if (GnomeDetector.IsRunningOnGnome())
        {
            try
            {
                this.InstallGnomeExtensionFiles();
            } catch (Exception e)
            {
                logger.LogError(e, "Exception when trying to deploy the GNOME extension files");
            }
        }
    }

    private void InstallGnomeExtensionFiles()
    {
        var versionDirectory = GnomeDetector.TryGetGnomeVersion() >= GnomeVersionForExtensionV2
            ? GnomeExtensionV2Directory
            : GnomeExtensionV1Directory;

        var gnomeExtensionDirectory = this.FileSystem.Path.Combine(
            AppContext.BaseDirectory,
            GnomeExtensionsDirectory,
            versionDirectory);

        var sourceExtensionFile = this.FileSystem.Path.Combine(gnomeExtensionDirectory, GnomeExtensionJsFile);
        var sourceMetadataFile = this.FileSystem.Path.Combine(gnomeExtensionDirectory, GnomeExtensionMetadataFile);

        var targetDirectory = this.FileSystem.Directory.Exists(GlobalGnomeExtensionTargetDirectory)
            ? GlobalGnomeExtensionTargetDirectory
            : this.CreateLocalGnomeExtensionTargetDirectory();

        var targetExtensionFile = Path.Combine(targetDirectory, GnomeExtensionJsFile);
        var targetMetadataFile = Path.Combine(targetDirectory, GnomeExtensionMetadataFile);

        this.FileSystem.File.Copy(sourceExtensionFile, targetExtensionFile, overwrite: true);
        this.FileSystem.File.Copy(sourceMetadataFile, targetMetadataFile, overwrite: true);
    }

    private string CreateLocalGnomeExtensionTargetDirectory()
    {
        var directory = this.FileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            LocalGnomeExtensionTargetDirectory);

        this.FileSystem.Directory.CreateDirectory(directory);
        return directory;
    }
}

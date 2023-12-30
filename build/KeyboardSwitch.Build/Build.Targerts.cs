using Serilog;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    public Target Clean => t => t
        .Description("Cleans the build outputs")
        .Executes(() =>
        {
            foreach (var project in this.GetProjects(includeInstaller: true))
            {
                Log.Information("Cleaning project {Name}", project.Name);
                DotNetClean(s => s
                    .SetProject(project)
                    .SetRuntime(this.RuntimeIdentifer)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform)
                    .SetProperty(nameof(TargetOS), this.TargetOS));
            }
        });

    public Target Compile => t => t
        .Description("Builds the project")
        .DependsOn(this.Clean)
        .Executes(() =>
        {
            foreach (var project in this.GetProjects())
            {
                Log.Information("Building project {Name}", project.Name);
                DotNetBuild(s => s
                    .SetProjectFile(project)
                    .SetRuntime(this.RuntimeIdentifer)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform)
                    .SetProperty(nameof(TargetOS), this.TargetOS)
                    .SetSelfContained(this.IsSelfContained)
                    .SetPublishSingleFile(this.PublishSingleFile)
                    .SetContinuousIntegrationBuild(true));
            }
        });

    public Target Publish => t => t
        .Description("Publishes the project")
        .DependsOn(this.Compile)
        .Requires(() => this.Configuration == Configuration.Release)
        .Executes(() =>
        {
            Log.Information("Cleaning the publish directory");
            PublishOutputDirectory.CreateOrCleanDirectory();

            this.PublishProject(this.Solution.KeyboardSwitch);
            this.PublishProject(this.Solution.KeyboardSwitch_Settings);

            Log.Information("Deleting unneeded files after publish");

            var appSettingsFile = this.PlatformDependent(
                windows: AppSettingsWindows,
                macos: AppSettingsMacOS,
                linux: AppSettingsLinux);

            appSettingsFile.Rename(AppSettings);

            foreach (var file in new[] { AppSettingsWindows, AppSettingsMacOS, AppSettingsLinux }
                .Where(f => f != appSettingsFile))
            {
                file.DeleteFile();
            }
        });

    public Target PreCreateArchive => t => t
        .Description("Copies additional files to the publish directory")
        .DependentFor(this.CreateArchive)
        .OnlyWhenStatic(() => this.TargetOS == TargetOS.Linux)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Copying additional files to the publish directory");

            CopyFileToDirectory(this.SourceLinuxInstallFile, PublishOutputDirectory);
            CopyFileToDirectory(this.SourceLinuxUninstallFile, PublishOutputDirectory);
        });

    public Target CreateArchive => t => t
        .Description("Creates an archive file containing the published project")
        .DependsOn(this.Publish)
        .Executes(() =>
        {
            var archiveFile = this.ArchiveFormat == ArchiveFormat.Tar ? this.TarFile : this.ZipFile;
            Log.Information("Archiving the publish output into {ArchiveFile}", archiveFile);

            archiveFile.DeleteFile();
            PublishOutputDirectory.CompressTo(archiveFile);
        })
        .Produces(this.ArchiveFormat == ArchiveFormat.Tar ? this.TarFile : this.ZipFile);

    public Target CreateDebianPackage => t => t
        .Description("Creates a Debian package containing the published project")
        .DependsOn(this.Publish)
        .Requires(() => OperatingSystem.IsLinux(), () => this.TargetOS == TargetOS.Linux)
        .Executes(() =>
        {
            Log.Information("Creating a Debian package containing the published project");

            this.DebDirectory.CreateOrCleanDirectory();

            this.DebConfigDirectory.CreateDirectory();

            CopyFile(this.SourceDebControlFile, this.TargetDebControlFile);
            CopyFile(this.SourceDebPostInstallFile, this.TargetDebPostInstallFile);
            CopyFile(this.SourceDebPreRemoveFile, this.TargetDebPreRemoveFile);
            CopyFile(this.SourceDebPostRemoveFile, this.TargetDebPostRemoveFile);

            this.TargetDebControlFile.UpdateText(text => text
                .Replace(VersionPlaceholder, Version)
                .Replace(ArchitecturePlaceholder, this.Platform.Deb));

            const string readAndExecute = "555";

            this.TargetDebPostInstallFile.SetUnixPermissions(readAndExecute);
            this.TargetDebPreRemoveFile.SetUnixPermissions(readAndExecute);
            this.TargetDebPostRemoveFile.SetUnixPermissions(readAndExecute);

            CopyDirectoryRecursively(PublishOutputDirectory, this.DebKeyboardSwitchDirectory);

            CopyFileToDirectory(this.SourceDebCopyrightFile, this.DebDocsDirectory, createDirectories: true);

            CopyFile(this.SourceLinuxIconFile, this.TargetDebIconFile);
            this.TargetDebIconFile.SetUnixPermissions("644");

            DebianPackageArchiveTool($"--build --root-owner-group {this.DebDirectory}");

            this.DebDirectory.DeleteDirectory();
        })
        .Produces(this.DebFile);

    public Target LocalCleanUp => t => t
        .Description("Deletes the publish directory")
        .TriggeredBy(this.CreateArchive, this.CreateDebianPackage)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting the publish directory");
            PublishOutputDirectory.DeleteDirectory();
        });
}

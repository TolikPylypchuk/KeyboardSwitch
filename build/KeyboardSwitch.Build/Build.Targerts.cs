using Nuke.Common.IO;

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

    public Target PrePublish => t => t
        .Description("Cleans the publish directory")
        .DependentFor(this.Publish)
        .After(this.Compile)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Cleaning the publish directory");
            PublishOutputDirectory.CreateOrCleanDirectory();
        });

    public Target Publish => t => t
        .Description("Publishes the project")
        .DependsOn(this.Compile)
        .Requires(() => this.Configuration == Configuration.Release)
        .Executes(() =>
        {
            this.PublishProject(this.Solution.KeyboardSwitch);
            this.PublishProject(this.Solution.KeyboardSwitch_Settings);
        });

    public Target PostPublish => t => t
        .Description("Deletes unneeded files in the publish directory")
        .TriggeredBy(this.Publish)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting unneeded files after publish for Windows");

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
        .After(this.PostPublish)
        .OnlyWhenStatic(() => this.TargetOS == TargetOS.Linux)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Copying additional files to the publish directory");

            CopyFile(this.SourceLinuxInstallFile, this.TargetLinuxInstallFile);
            CopyFile(this.SourceLinuxUninstallFile, this.TargetLinuxUninstallFile);
        });

    public Target CreateArchive => t => t
        .Description("Creates an archive file containing the published project")
        .DependsOn(this.Publish)
        .After(this.PostPublish)
        .Executes(() =>
        {
            var archiveFile = this.ArchiveFormat == ArchiveFormat.Tar ? this.TarFile : this.ZipFile;
            Log.Information("Archiving the publish output into {ArchiveFile}", archiveFile);

            archiveFile.DeleteFile();
            PublishOutputDirectory.CompressTo(archiveFile);
        })
        .Produces(this.ArchiveFormat == ArchiveFormat.Tar ? this.TarFile : this.ZipFile);

    public Target PostCreateArchive => t => t
        .Description("Deletes the publish directory")
        .TriggeredBy(this.CreateArchive)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting the publish directory");
            PublishOutputDirectory.DeleteDirectory();
        });

    public Target CreateDebianPackage => t => t
        .Description("Creates a Debian package containing the published project")
        .DependsOn(this.Publish)
        .After(this.PostPublish)
        .Requires(() => OperatingSystem.IsLinux(), () => this.TargetOS == TargetOS.Linux)
        .Executes(() =>
        {
        })
        .Produces(this.DebFile);
}

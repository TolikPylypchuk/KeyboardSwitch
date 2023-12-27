using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.FileSystemTasks;

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

    public Target PostPublishWindows => t => t
        .Description("Deletes unneeded files in the publish directory for Windows")
        .TriggeredBy(this.Publish)
        .OnlyWhenStatic(() => this.TargetOS == TargetOS.Windows)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting unneeded files after publish for Windows");

            PngIcon.DeleteFile();
            AppleIcon.DeleteFile();

            AppSettingsWindows.Rename(AppSettings);
            AppSettingsMacOS.DeleteFile();
            AppSettingsLinux.DeleteFile();
        });

    public Target PostPublishMacOS => t => t
        .Description("Deletes unneeded files in the publish directory for macOS")
        .TriggeredBy(this.Publish)
        .OnlyWhenStatic(() => this.TargetOS == TargetOS.MacOS)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting unneeded files after publish for macOS");

            PngIcon.DeleteFile();

            AppSettingsWindows.DeleteFile();
            AppSettingsMacOS.Rename(AppSettings);
            AppSettingsLinux.DeleteFile();
        });

    public Target PostPublishLinux => t => t
        .Description("Deletes unneeded files in the publish directory for Linux")
        .TriggeredBy(this.Publish)
        .OnlyWhenStatic(() => this.TargetOS == TargetOS.Linux)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting unneeded files after publish for Linux");

            AppleIcon.DeleteFile();

            AppSettingsWindows.DeleteFile();
            AppSettingsMacOS.DeleteFile();
            AppSettingsLinux.Rename(AppSettings);
        });

    public Target PreCreateArchive => t => t
        .Description("Copies additional files to the publish directory")
        .DependentFor(this.CreateArchive)
        .After(this.PostPublishWindows)
        .After(this.PostPublishMacOS)
        .After(this.PostPublishLinux)
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
        .After(this.PostPublishWindows)
        .After(this.PostPublishMacOS)
        .After(this.PostPublishLinux)
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

    private IEnumerable<Project> GetProjects(bool includeInstaller = false)
    {
        yield return this.Solution.KeyboardSwitch_Core;

        yield return this.TargetOS switch
        {
            var os when os == TargetOS.MacOS => this.Solution.KeyboardSwitch_MacOS,
            var os when os == TargetOS.Linux => this.Solution.KeyboardSwitch_Linux,
            _ => this.Solution.KeyboardSwitch_Windows
        };

        yield return this.Solution.KeyboardSwitch;

        yield return this.Solution.KeyboardSwitch_Settings_Core;
        yield return this.Solution.KeyboardSwitch_Settings;

        if (includeInstaller)
        {
            yield return this.Solution.KeyboardSwitch_Windows_Installer;
        }
    }

    private void PublishProject(Project project)
    {
        Log.Information("Publishing project {Name}", project.Name);

        DotNetPublish(s => s
            .SetProject(project)
            .SetRuntime(this.RuntimeIdentifer)
            .SetConfiguration(this.Configuration)
            .SetPlatform(this.Platform)
            .SetProperty(nameof(TargetOS), this.TargetOS)
            .SetNoBuild(true)
            .SetOutput(PublishOutputDirectory)
            .SetSelfContained(this.IsSelfContained)
            .SetPublishSingleFile(this.PublishSingleFile));
    }
}

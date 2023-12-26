using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

using Serilog;

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
                    .SetSelfContained(this.PublishSingleFile || this.Configuration == Configuration.Release)
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
            PublishOutputPath.CreateOrCleanDirectory();
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
            Log.Information("Deleting unneeded files after publish");

            switch (this.TargetOS)
            {
                case var os when os == TargetOS.Windows:
                    PngIcon.DeleteFile();
                    AppleIcon.DeleteFile();
                    AppSettingsWindows.Rename(AppSettings);
                    AppSettingsMacOS.DeleteFile();
                    AppSettingsLinux.DeleteFile();
                    break;
                case var os when os == TargetOS.MacOS:
                    PngIcon.DeleteFile();
                    AppSettingsWindows.DeleteFile();
                    AppSettingsMacOS.Rename(AppSettings);
                    AppSettingsLinux.DeleteFile();
                    break;
                case var os when os == TargetOS.Linux:
                    AppleIcon.DeleteFile();
                    AppSettingsWindows.DeleteFile();
                    AppSettingsMacOS.DeleteFile();
                    AppSettingsLinux.Rename(AppSettings);
                    break;
            }
        });

    public Target CreateZip => t => t
        .Description("Creates a zip-file containing the published project")
        .DependsOn(this.Publish)
        .After(this.PostPublish)
        .Executes(() =>
        {
            Log.Information("Zipping the publish output into {ZipFile}", this.ZipFile);
            this.ZipFile.DeleteFile();
            PublishOutputPath.ZipTo(this.ZipFile);
        })
        .Produces(this.ZipFile);

    public Target PostCreateZip => t => t
        .Description("Deletes the publish directory")
        .TriggeredBy(this.CreateZip)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting the publish directory");
            PublishOutputPath.DeleteDirectory();
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
            .SetOutput(PublishOutputPath)
            .SetSelfContained(true)
            .SetPublishSingleFile(this.PublishSingleFile));
    }
}

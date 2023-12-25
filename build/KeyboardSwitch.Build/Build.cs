using System.Runtime.InteropServices;

using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build : NukeBuild
{
    private static readonly AbsolutePath PublishOutputPath = RootDirectory / "artifacts" / "publish";

    private static readonly AbsolutePath PngIcon = PublishOutputPath / "icon.png";
    private static readonly AbsolutePath AppleIcon = PublishOutputPath / "KeyboardSwitch.icns";

    private static readonly AbsolutePath AppSettingsWindows = PublishOutputPath / "appsettings.windows.json";
    private static readonly AbsolutePath AppSettingsMacOS = PublishOutputPath / "appsettings.macos.json";
    private static readonly AbsolutePath AppSettingsLinux = PublishOutputPath / "appsettings.linux.json";

    private static readonly string AppSettings = "appsettings.json";

    [Parameter("Configuration to build - 'Debug' (local) or 'Release' (server) by default")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Target OS - current OS by default")]
    private readonly TargetOS TargetOS =
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? TargetOS.MacOS
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? TargetOS.Linux
                : TargetOS.Windows;

    [Parameter("Platform - current architecture by default")]
    private readonly Platform Platform =
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? Platform.Arm64 : Platform.X64;

    [Parameter("Publish single file - false by default")]
    private readonly bool PublishSingleFile = false;

    [Solution(GenerateProjects = true)]
    private readonly Solution Solution = new();

    private string RuntimeIdentifer =>
        $"{this.TargetOS.RuntimeIdentifierPart}-{this.Platform.RuntimeIdentifierPart}";

    public Target Clean => t => t
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
        .DependentFor(this.Publish)
        .After(this.Compile)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Cleaning the publish directory");
            PublishOutputPath.CreateOrCleanDirectory();
        });

    public Target Publish => t => t
        .DependsOn(this.Compile)
        .Requires(() => this.Configuration == Configuration.Release)
        .Executes(() =>
        {
            this.PublishProject(this.Solution.KeyboardSwitch);
            this.PublishProject(this.Solution.KeyboardSwitch_Settings);
        });

    public Target PostPublish => t => t
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

    public static int Main() =>
        Execute<Build>(x => x.Compile);

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

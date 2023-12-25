using System.Runtime.InteropServices;

using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build : NukeBuild
{
    [Parameter("Configuration to build - by default 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Target OS (current OS by default)")]
    private readonly TargetOS? TargetOS;

    [Parameter("Platform (current architecture by default)")]
    private readonly Platform Platform =
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? Platform.Arm64 : Platform.X64;

    [Solution(GenerateProjects = true)]
    private readonly Solution Solution = new();

    private readonly AbsolutePath PublishOutputPath = RootDirectory / "artifacts" / "publish";

    private string RuntimeIdentifer =>
        $"{(this.TargetOS ?? this.GetCurrentOS()).RuntimeIdentifierPart}-{this.Platform.RuntimeIdentifierPart}";

    private List<Project> ProjectsInBuildOrder =>
    [
        this.Solution.KeyboardSwitch_Core,
        this.Solution.KeyboardSwitch_Windows,
        this.Solution.KeyboardSwitch_MacOS,
        this.Solution.KeyboardSwitch_Linux,
        this.Solution.KeyboardSwitch,
        this.Solution.KeyboardSwitch_Settings_Core,
        this.Solution.KeyboardSwitch_Settings
    ];

    public Target Clean => t => t
        .Executes(() =>
        {
            foreach (var project in this.ProjectsInBuildOrder.Append(this.Solution.KeyboardSwitch_Windows_Installer))
            {
                Log.Information("Cleaning project {Name}", project.Name);
                DotNetClean(s => s
                    .SetProject(project)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform)
                    .SetProperty(nameof(TargetOS), this.TargetOS));
            }
        });

    public Target Compile => t => t
        .DependsOn(this.Clean)
        .Executes(() =>
        {
            foreach (var project in this.ProjectsInBuildOrder)
            {
                Log.Information("Building project {Name}", project.Name);
                DotNetBuild(s => s
                    .SetProjectFile(project)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform)
                    .SetProperty(nameof(TargetOS), this.TargetOS));
            }
        });

    public Target Publish => t => t
        .DependsOn(this.Compile)
        .Requires(() => this.Configuration == Configuration.Release)
        .Executes(() =>
        {
            PublishOutputPath.CreateOrCleanDirectory();

            this.PublishProject(this.Solution.KeyboardSwitch);
            this.PublishProject(this.Solution.KeyboardSwitch_Settings);
        });

    public static int Main() =>
        Execute<Build>(x => x.Compile);

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
            .SetOutput(this.PublishOutputPath)
            .SetSelfContained(true));
    }

    private TargetOS GetCurrentOS() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? TargetOS.MacOS
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? TargetOS.Linux
                : TargetOS.Windows;
}

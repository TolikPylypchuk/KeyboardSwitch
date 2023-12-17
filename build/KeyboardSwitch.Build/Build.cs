using System.Runtime.InteropServices;

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

    public Target Clean => t => t
        .Executes(() =>
        {
            this.ForEachProject(project =>
            {
                Log.Information("Cleaning project {Name}", project.Name);
                DotNetClean(s => s
                    .SetProject(project)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform)
                    .SetProperty(nameof(TargetOS), this.TargetOS));
            });
        });

    public Target Publish => t => t
        .DependsOn(Clean)
        .Executes(() =>
        {
            var os = (this.TargetOS ?? this.GetCurrentOS()).RuntimeIdentifierPart;
            var arch = this.Platform.RuntimeIdentifierPart;
            var runtime = $"{os}-{arch}";

            this.PublishProject(this.Solution.KeyboardSwitch, runtime);
            this.PublishProject(this.Solution.KeyboardSwitch_Settings, runtime);
        });

    public static int Main() =>
        Execute<Build>(x => x.Publish);

    private void ForEachProject(Action<Project> action)
    {
        foreach (var project in this.Solution.Projects
                .Where(project => project != this.Solution.KeyboardSwitch_Build &&
                    project != this.Solution.KeyboardSwitch_Windows_Setup))
        {
            action(project);
        }
    }

    private void PublishProject(Project project, string runtime) =>
        DotNetPublish(s => s
            .SetProject(project)
            .SetRuntime(runtime)
            .SetConfiguration(this.Configuration)
            .SetPlatform(this.Platform)
            .SetProperty(nameof(TargetOS), this.TargetOS)
            .SetSelfContained(true));

    private TargetOS GetCurrentOS() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? TargetOS.MacOS
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? TargetOS.Linux
                : TargetOS.Windows;
}

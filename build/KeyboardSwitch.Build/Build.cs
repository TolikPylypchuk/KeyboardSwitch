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
        .Before(Restore)
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

    public Target Restore => t => t
        .DependsOn(Clean)
        .Executes(() =>
        {
            this.ForEachProject(project =>
            {
                Log.Information("Restoring project {Name}", project.Name);
                DotNetRestore(s => s
                    .SetProjectFile(project)
                    .SetPlatform(this.Platform)
                    .SetProperty(nameof(TargetOS), this.TargetOS));
            });
        });

    public Target Compile => t => t
        .DependsOn(Restore)
        .Executes(() =>
        {
        });

    public static int Main() =>
        Execute<Build>(x => x.Compile);

    private void ForEachProject(Action<Project> action)
    {
        foreach (var project in this.Solution.Projects
                .Where(project => project != this.Solution.KeyboardSwitch_Build))
        {
            action(project);
        }
    }
}

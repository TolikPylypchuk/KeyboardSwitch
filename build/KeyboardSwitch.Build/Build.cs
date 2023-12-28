using System.Reflection;

using Nuke.Common.ProjectModel;

public partial class Build : NukeBuild
{
    private static readonly string? Version = Assembly.GetExecutingAssembly()?.GetName().Version?.ToString(3);

    [Solution(GenerateProjects = true)]
    private readonly Solution Solution = new();

    public static int Main() =>
        Execute<Build>(x => x.Compile);
}

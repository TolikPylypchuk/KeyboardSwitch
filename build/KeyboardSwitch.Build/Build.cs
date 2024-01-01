using System.Reflection;

public partial class Build : NukeBuild
{
    private static readonly string? Version = Assembly.GetExecutingAssembly()?.GetName().Version?.ToString(3);
    private static readonly string ReleaseNumber = "1";

    private static readonly string VersionPlaceholder = "$VERSION";
    private static readonly string ReleasePlaceholder = "$RELEASE";
    private static readonly string ArchitecturePlaceholder = "$ARCH";
    private static readonly string OutputPlaceholder = "$OUTPUT";

    [Solution(GenerateProjects = true)]
    private readonly Solution Solution = new();

    public static int Main() =>
        Execute<Build>(x => x.Compile);
}

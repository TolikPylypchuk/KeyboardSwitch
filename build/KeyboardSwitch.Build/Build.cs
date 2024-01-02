using System.Reflection;

public partial class Build : NukeBuild
{
    private static readonly Version? AssemblyVersion = Assembly.GetExecutingAssembly()?.GetName().Version;
    private static readonly string? Version = AssemblyVersion?.ToString(3);
    private static readonly string MajorVersion = AssemblyVersion?.Major.ToString() ?? "0";
    private static readonly string MinorVersion = AssemblyVersion?.Minor.ToString() ?? "0";
    private static readonly string ReleaseNumber = "1";

    private static readonly string VersionPlaceholder = "$VERSION";
    private static readonly string MajorVersionPlaceholder = "$MAJOR_VERSION";
    private static readonly string MinorVersionPlaceholder = "$MINOR_VERSION";
    private static readonly string ReleasePlaceholder = "$RELEASE";
    private static readonly string ArchitecturePlaceholder = "$ARCH";
    private static readonly string OutputPlaceholder = "$OUTPUT";

    [Solution(GenerateProjects = true)]
    private readonly Solution Solution = new();

    public static int Main() =>
        Execute<Build>(x => x.Compile);
}

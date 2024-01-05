using System.Reflection;

public partial class Build : NukeBuild
{
    private static readonly Version? AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
    private static readonly string? Version = AssemblyVersion?.ToString(3);
    private static readonly string MajorVersion = AssemblyVersion?.Major.ToString() ?? "0";
    private static readonly string MinorVersion = AssemblyVersion?.Minor.ToString() ?? "0";
    private const string ReleaseNumber = "1";

    private const string VersionPlaceholder = "$VERSION";
    private const string MajorVersionPlaceholder = "$MAJOR_VERSION";
    private const string MinorVersionPlaceholder = "$MINOR_VERSION";
    private const string ReleasePlaceholder = "$RELEASE";
    private const string ArchitecturePlaceholder = "$ARCH";
    private const string OutputPlaceholder = "$OUTPUT";

    [Solution(GenerateProjects = true)]
    private readonly Solution Solution = new();

    public static int Main() =>
        Execute<Build>(x => x.Compile);
}

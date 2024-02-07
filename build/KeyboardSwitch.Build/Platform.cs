[Serializable]
[TypeConverter(typeof(TypeConverter<Platform>))]
public class Platform : Enumeration
{
    public const string X64Value = "x64";
    public const string Arm64Value = "arm64";

    public static Platform X64 = new()
    {
        Value = X64Value,
        MSBuild = "x64",
        RuntimeIdentifierPart = "x64",
        Archive = "x64",
        Msi = "x64",
        Pkg = "x86_64",
        Deb = "amd64",
        Rpm = "x86_64"
    };

    public static Platform Arm64 = new()
    {
        Value = Arm64Value,
        MSBuild = "ARM64",
        RuntimeIdentifierPart = "arm64",
        Archive = "arm64",
        Msi = "arm64",
        Pkg = "arm64",
        Deb = "arm64",
        Rpm = "aarch64"
    };

    public required string MSBuild { get; init; }
    public required string RuntimeIdentifierPart { get; init; }

    public required string Archive { get; init; }
    public required string Msi { get; init; }
    public required string Pkg { get; init; }
    public required string Deb { get; init; }
    public required string Rpm { get; init; }
}

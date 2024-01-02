[Serializable]
[TypeConverter(typeof(TypeConverter<Platform>))]
public class Platform : Enumeration
{
    public static Platform X64 = new()
    {
        Value = "x64",
        RuntimeIdentifierPart = "x64",
        Archive = "x64",
        Pkg = "x86_64",
        Deb = "amd64",
        Rpm = "x86_64"
    };

    public static Platform Arm64 = new()
    {
        Value = "ARM64",
        RuntimeIdentifierPart = "arm64",
        Archive = "arm64",
        Pkg = "arm64",
        Deb = "arm64",
        Rpm = "aarch64"
    };

    public required string RuntimeIdentifierPart { get; init; }

    public required string Archive { get; init; }
    public required string Pkg { get; init; }
    public required string Deb { get; init; }
    public required string Rpm { get; init; }
}

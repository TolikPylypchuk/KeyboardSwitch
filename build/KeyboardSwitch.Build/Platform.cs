[Serializable]
[TypeConverter(typeof(TypeConverter<Platform>))]
public class Platform : Enumeration
{
    public static Platform X64 = new()
    {
        Value = "x64",
        RuntimeIdentifierPart = "x64",
        Zip = "x64",
        Tar = "x64",
        Deb = "amd64"
    };

    public static Platform Arm64 = new()
    {
        Value = "ARM64",
        RuntimeIdentifierPart = "arm64",
        Zip = "arm64",
        Tar = "arm64",
        Deb = "arm64"
    };

    public required string RuntimeIdentifierPart { get; init; }

    public required string Zip { get; init; }
    public required string Tar { get; init; }
    public required string Deb { get; init; }
}

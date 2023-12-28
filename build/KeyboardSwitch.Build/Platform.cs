[Serializable]
[TypeConverter(typeof(TypeConverter<Platform>))]
public class Platform : Enumeration
{
    public static Platform X64 = new()
    {
        Value = "x64",
        RuntimeIdentifierPart = "x64",
        ZipPart = "x64",
        TarPart = "x64",
        DebPart = "amd64"
    };

    public static Platform Arm64 = new()
    {
        Value = "ARM64",
        RuntimeIdentifierPart = "arm64",
        ZipPart = "arm64",
        TarPart = "arm64",
        DebPart = "arm64"
    };

    public required string RuntimeIdentifierPart { get; init; }
    public required string ZipPart { get; init; }
    public required string TarPart { get; init; }
    public required string DebPart { get; init; }
}

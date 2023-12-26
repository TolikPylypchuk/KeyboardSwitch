[Serializable]
[TypeConverter(typeof(TypeConverter<Platform>))]
public class Platform : Enumeration
{
    public static Platform X64 = new()
    {
        Value = "x64",
        RuntimeIdentifierPart = "x64",
        ZipPart = "x64"
    };

    public static Platform Arm64 = new()
    {
        Value = "ARM64",
        RuntimeIdentifierPart = "arm64",
        ZipPart = "arm64"
    };

    public required string RuntimeIdentifierPart { get; init; }
    public required string ZipPart { get; init; }
}

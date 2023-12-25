[Serializable]
[TypeConverter(typeof(TypeConverter<Configuration>))]
public class TargetOS : Enumeration
{
    public static TargetOS Windows = new() { Value = "Windows", RuntimeIdentifierPart = "win" };
    public static TargetOS MacOS = new() { Value = "macOS", RuntimeIdentifierPart = "osx" };
    public static TargetOS Linux = new() { Value = "Linux", RuntimeIdentifierPart = "linux" };

    public required string RuntimeIdentifierPart { get; init; }

    public static implicit operator string(TargetOS targetOS) =>
        targetOS.Value;
}

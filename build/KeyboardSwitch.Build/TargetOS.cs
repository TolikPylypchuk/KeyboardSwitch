[Serializable]
[TypeConverter(typeof(TypeConverter<TargetOS>))]
public class TargetOS : Enumeration
{
    public const string WindowsValue = "Windows";
    public const string MacOSValue = "macOS";
    public const string LinuxValue = "Linux";

    public static TargetOS Windows = new() { Value = WindowsValue, RuntimeIdentifierPart = "win" };
    public static TargetOS MacOS = new() { Value = MacOSValue, RuntimeIdentifierPart = "osx" };
    public static TargetOS Linux = new() { Value = LinuxValue, RuntimeIdentifierPart = "linux" };

    public required string RuntimeIdentifierPart { get; init; }
}

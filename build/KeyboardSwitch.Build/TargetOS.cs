[TypeConverter(typeof(TypeConverter<Configuration>))]
public class TargetOS : Enumeration
{
    public static TargetOS Windows = new() { Value = "Windows" };
    public static TargetOS MacOS = new() { Value = "macOS" };
    public static TargetOS Linux = new() { Value = "Linux" };

    public static implicit operator string(TargetOS targetOS) =>
        targetOS.Value;
}

[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Platform : Enumeration
{
    public static Platform X64 = new() { Value = "x64" };
    public static Platform Arm64 = new() { Value = "ARM64" };

    public static implicit operator string(Platform platform) =>
        platform.Value;
}

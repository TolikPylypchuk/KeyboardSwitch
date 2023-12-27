[Serializable]
[TypeConverter(typeof(TypeConverter<ArchiveFormat>))]
public class ArchiveFormat : Enumeration
{
    public static ArchiveFormat Zip = new() { Value = "zip" };
    public static ArchiveFormat Tar = new() { Value = "tar" };
}

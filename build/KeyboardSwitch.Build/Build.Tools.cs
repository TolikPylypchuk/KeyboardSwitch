public partial class Build : NukeBuild
{
    [PathVariable("dpkg-deb")]
    private readonly Tool DebianPackageArchiveTool = null!;
}

public partial class Build : NukeBuild
{
    [PathVariable("dpkg-deb")]
    private readonly Tool? DebianPackageArchiveTool;

    [PathVariable("rpmbuild")]
    private readonly Tool? BuildRpmTool;
}

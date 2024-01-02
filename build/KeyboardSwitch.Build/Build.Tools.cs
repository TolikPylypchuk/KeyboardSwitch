public partial class Build : NukeBuild
{
    [PathVariable("codesign")]
    private readonly Tool? CodeSign;

    [PathVariable("pkgbuild")]
    private readonly Tool? PkgBuild;

    [PathVariable("productbuild")]
    private readonly Tool? ProductBuild;

    [PathVariable("xcrun")]
    private readonly Tool? XCodeRun;

    [PathVariable("dpkg-deb")]
    private readonly Tool? DebianPackageTool;

    [PathVariable("rpmbuild")]
    private readonly Tool? RpmBuild;
}

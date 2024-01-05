public partial class Build
{
    [PathVariable("codesign")]
    private readonly Tool? codeSign;

    [PathVariable("pkgbuild")]
    private readonly Tool? pkgBuild;

    [PathVariable("productbuild")]
    private readonly Tool? productBuild;

    [PathVariable("xcrun")]
    private readonly Tool? xCodeRun;

    [PathVariable("dpkg-deb")]
    private readonly Tool? debianPackageTool;

    [PathVariable("rpmbuild")]
    private readonly Tool? rpmBuild;

    private Tool CodeSign =>
        this.codeSign.NotNull("codesign is not available")!;

    private Tool PkgBuild =>
        this.pkgBuild.NotNull("pkgbuild is not available")!;

    private Tool ProductBuild =>
        this.productBuild.NotNull("productbuild is not available")!;

    private Tool XCodeRun =>
        this.xCodeRun.NotNull("xcrun is not available")!;

    private Tool DebianPackageTool =>
        this.debianPackageTool.NotNull("dpkg-deb is not available")!;

    private Tool RpmBuild =>
        this.rpmBuild.NotNull("rpmbuild is not available")!;
}

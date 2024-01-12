public partial class Build
{
    private const string RpmSpecFile = $"{KeyboardSwitchLower}.spec";

    private static AbsolutePath RpmDirectory =>
        ArtifactsDirectory / "rpm";

    private static AbsolutePath TargetRpmLicenseFile =>
        ArtifactsDirectory / LicenseFile;

    private static AbsolutePath AnyRpmFile =>
        ArtifactsDirectory / "*.rpm";

    private string RpmFileName =>
        $"{KeyboardSwitchLower}-{Version}-{ReleaseNumber}.{this.Platform.Rpm}.rpm";

    private AbsolutePath RpmFile =>
        ArtifactsDirectory / RpmFileName;

    private AbsolutePath SourceRpmSpecFile =>
        this.LinuxFilesDirectory / RpmSpecFile;

    private AbsolutePath TargetRpmSpecFile =>
        ArtifactsDirectory / RpmSpecFile;

    private AbsolutePath RpmOutputFile =>
        RpmDirectory / "RPMS" / this.Platform.Rpm / RpmFileName;
}

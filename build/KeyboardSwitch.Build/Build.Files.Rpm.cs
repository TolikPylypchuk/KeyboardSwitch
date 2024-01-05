public partial class Build
{
    private const string RpmSpecFile = $"{KeyboardSwitchLower}.spec";

    private static AbsolutePath RpmDirectory =>
        ArtifactsDirectory / "rpm";

    private static AbsolutePath TargetRpmLicenseFile =>
        ArtifactsDirectory / LicenseFile;

    private string RpmFileName =>
        $"{KeyboardSwitchLower}-{Version}-{ReleaseNumber}.{this.Platform.Rpm}.rpm";

    private AbsolutePath SourceRpmSpecFile =>
        this.LinuxFilesDirectory / RpmSpecFile;

    private AbsolutePath TargetRpmSpecFile =>
        ArtifactsDirectory / RpmSpecFile;

    private AbsolutePath RpmOutputFile =>
        RpmDirectory / "RPMS" / this.Platform.Rpm / RpmFileName;

    private AbsolutePath RpmFile =>
        ArtifactsDirectory / RpmFileName;
}

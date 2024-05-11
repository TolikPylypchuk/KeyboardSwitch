public partial class Build
{
    private const string DebControlFile = "control";
    private const string DebPostInstallFile = "postinst";
    private const string DebPreRemoveFile = "prerm";
    private const string DebPostRemoveFile = "postrm";

    private static AbsolutePath AnyDebFile =>
        ArtifactsDirectory / "*.deb";

    private string DebFileName =>
        $"{KeyboardSwitchLower}_{Version}-{ReleaseNumber}_{this.Platform.Deb}";

    private AbsolutePath DebDirectory =>
        ArtifactsDirectory / this.DebFileName;

    private AbsolutePath DebFile =>
        this.WithSuffix(ArtifactsDirectory / $"{this.DebFileName}.deb");

    private AbsolutePath DebConfigDirectory =>
        this.DebDirectory / "DEBIAN";

    private AbsolutePath DebKeyboardSwitchDirectory =>
        this.DebDirectory / "opt" / KeyboardSwitchLower;

    private AbsolutePath DebDocsDirectory =>
        this.DebDirectory / "usr" / "share" / "doc" / KeyboardSwitchLower;

    private AbsolutePath SourceDebControlFile =>
        this.LinuxFilesDirectory / DebControlFile;

    private AbsolutePath TargetDebControlFile =>
        this.DebConfigDirectory / DebControlFile;

    private AbsolutePath SourceDebCopyrightFile =>
        this.LinuxFilesDirectory / "copyright";

    private AbsolutePath SourceDebPostInstallFile =>
        this.LinuxFilesDirectory / DebPostInstallFile;

    private AbsolutePath TargetDebPostInstallFile =>
        this.DebConfigDirectory / DebPostInstallFile;

    private AbsolutePath SourceDebPreRemoveFile =>
        this.LinuxFilesDirectory / DebPreRemoveFile;

    private AbsolutePath TargetDebPreRemoveFile =>
        this.DebConfigDirectory / DebPreRemoveFile;

    private AbsolutePath SourceDebPostRemoveFile =>
        this.LinuxFilesDirectory / DebPostRemoveFile;

    private AbsolutePath TargetDebPostRemoveFile =>
        this.DebConfigDirectory / DebPostRemoveFile;
}

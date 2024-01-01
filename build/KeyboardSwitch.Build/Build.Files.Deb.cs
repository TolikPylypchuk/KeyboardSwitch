public partial class Build
{
    private static readonly string DebControlFile = "control";
    private static readonly string DebPostInstallFile = "postinst";
    private static readonly string DebPreRemoveFile = "prerm";
    private static readonly string DebPostRemoveFile = "postrm";

    private string DebFileName =>
        $"{KeyboardSwitchLower}_{Version}-{ReleaseNumber}_{this.Platform.Deb}";

    private AbsolutePath DebDirectory =>
        ArtifactsDirectory / this.DebFileName;

    private AbsolutePath DebFile =>
        ArtifactsDirectory / $"{this.DebFileName}.deb";

    private AbsolutePath DebConfigDirectory =>
        this.DebDirectory / "DEBIAN";

    private AbsolutePath DebKeyboardSwitchDirectory =>
        this.DebDirectory / "opt" / KeyboardSwitchLower;

    private AbsolutePath DebDocsDirectory =>
        this.DebDirectory / "usr" / "share" / "doc" / KeyboardSwitchLower;

    private AbsolutePath DebIconsDirectory =>
        this.DebDirectory / "usr" / "share" / "icons" / "hicolor" / "512x512" / "app";

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

    private AbsolutePath TargetDebIconFile =>
        this.DebIconsDirectory / LinuxIconFile;
}

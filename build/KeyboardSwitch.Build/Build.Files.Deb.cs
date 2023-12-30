using System.Drawing;

public partial class Build
{
    private static readonly string DebControlFile = "control";
    private static readonly string DebPostInstallFile = "postinst";
    private static readonly string DebPreRemoveFile = "prerm";
    private static readonly string DebPostRemoveFile = "postrm";

    private string DebFileName =>
        $"{KeyboardSwitchLower}_{Version}-1_{this.Platform.Deb}";

    private AbsolutePath DebDirectory =>
        ArtifactsDirectory / DebFileName;

    private AbsolutePath DebFile =>
        ArtifactsDirectory / $"{DebFileName}.deb";

    private AbsolutePath DebConfigDirectory =>
        DebDirectory / "DEBIAN";

    private AbsolutePath DebKeyboardSwitchDirectory =>
        DebDirectory / "opt" / KeyboardSwitchLower;

    private AbsolutePath DebDocsDirectory =>
        DebDirectory / "usr" / "share" / "doc" / KeyboardSwitchLower;

    private AbsolutePath DebIconsDirectory =>
        DebDirectory / "usr" / "share" / "icons" / "hicolor" / "512x512" / "app";

    private AbsolutePath SourceDebControlFile =>
        LinuxFilesDirectory / DebControlFile;

    private AbsolutePath TargetDebControlFile =>
        DebConfigDirectory / DebControlFile;

    private AbsolutePath SourceDebCopyrightFile =>
        LinuxFilesDirectory / "copyright";

    private AbsolutePath SourceDebPostInstallFile =>
        LinuxFilesDirectory / DebPostInstallFile;

    private AbsolutePath TargetDebPostInstallFile =>
        DebConfigDirectory / DebPostInstallFile;

    private AbsolutePath SourceDebPreRemoveFile =>
        LinuxFilesDirectory / DebPreRemoveFile;

    private AbsolutePath TargetDebPreRemoveFile =>
        DebConfigDirectory / DebPreRemoveFile;

    private AbsolutePath SourceDebPostRemoveFile =>
        LinuxFilesDirectory / DebPostRemoveFile;

    private AbsolutePath TargetDebPostRemoveFile =>
        DebConfigDirectory / DebPostRemoveFile;

    private AbsolutePath TargetDebIconFile =>
        DebIconsDirectory / LinuxIconFile;
}

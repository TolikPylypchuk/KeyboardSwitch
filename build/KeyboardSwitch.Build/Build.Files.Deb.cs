using Nuke.Common.IO;

public partial class Build
{
    private static readonly string DebControlFile = "control";
    private static readonly string DebCopyrightFile = "copyright";
    private static readonly string DebPostInstallFile = "postinst";
    private static readonly string DebPreRemoveFile = "prerm";

    private string DebFileName =>
        $"{KeyboardSwitchLower}_{Version}-1_{this.Platform.DebPart}";

    private AbsolutePath DebRootDirectory =>
        ArtifactsDirectory / DebFileName;

    private AbsolutePath DebFile =>
        ArtifactsDirectory / $"{DebFileName}.deb";

    private AbsolutePath DebConfigDirectory =>
        DebRootDirectory / "DEBIAN";

    private AbsolutePath SourceDebControlFile =>
        LinuxFilesDirectory / DebControlFile;

    private AbsolutePath TargetDebControlFile =>
        DebConfigDirectory / DebControlFile;

    private AbsolutePath SourceDebCopyrightFile =>
        LinuxFilesDirectory / DebCopyrightFile;

    private AbsolutePath TargetDebCopyrightFile =>
        DebConfigDirectory / DebCopyrightFile;

    private AbsolutePath SourceDebPostInstallFile =>
        LinuxFilesDirectory / DebPostInstallFile;

    private AbsolutePath TargetDebPostInstallFile =>
        DebConfigDirectory / DebPostInstallFile;

    private AbsolutePath SourceDebPreRemoveFile =>
        LinuxFilesDirectory / DebPreRemoveFile;

    private AbsolutePath TargetDebPreRemoveFile =>
        DebConfigDirectory / DebPreRemoveFile;

    private AbsolutePath TargetOutputDirectory =>
        DebRootDirectory / "opt" / KeyboardSwitchLower;
}

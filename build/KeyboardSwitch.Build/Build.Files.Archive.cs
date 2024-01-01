public partial class Build
{
    private AbsolutePath ArchiveFile =>
        ArtifactsDirectory /
            $"{KeyboardSwitch}-{Version}-{this.Platform.Archive}." +
            $"{(this.ArchiveFormat == ArchiveFormat.Tar ? "tar.gz" : "zip")}";

    private AbsolutePath SourceLinuxInstallFile =>
        this.LinuxFilesDirectory / "install.sh";

    private AbsolutePath SourceLinuxUninstallFile =>
        this.LinuxFilesDirectory / "uninstall.sh";
}

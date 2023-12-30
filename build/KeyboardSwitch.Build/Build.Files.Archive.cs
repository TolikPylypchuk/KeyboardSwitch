public partial class Build
{
    private AbsolutePath ZipFile =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.Zip}.zip";

    private AbsolutePath TarFile =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.Tar}.tar.gz";

    private AbsolutePath SourceLinuxInstallFile =>
        this.LinuxFilesDirectory / "install.sh";

    private AbsolutePath SourceLinuxUninstallFile =>
        this.LinuxFilesDirectory / "uninstall.sh";
}

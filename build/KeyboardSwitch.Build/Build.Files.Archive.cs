public partial class Build
{
    private static AbsolutePath AnyZipFile =>
        ArtifactsDirectory / $"*.zip";

    private static AbsolutePath AnyTarFile =>
        ArtifactsDirectory / $"*.tar.gz";

    private AbsolutePath ZipFile =>
        GetArchiveFile("zip");

    private AbsolutePath TarFile =>
        GetArchiveFile("tar.gz");

    private AbsolutePath SourceLinuxInstallFile =>
        this.LinuxFilesDirectory / "install.sh";

    private AbsolutePath SourceLinuxUninstallFile =>
        this.LinuxFilesDirectory / "uninstall.sh";

    private AbsolutePath GetArchiveFile(string format) =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.Archive}.{format}";
}

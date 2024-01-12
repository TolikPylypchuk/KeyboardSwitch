public partial class Build
{
    private const string ZipFormat = "zip";
    private const string TarFormat = "tar.gz";

    private static AbsolutePath AnyZipFile =>
        ArtifactsDirectory / $"*.{ZipFormat}";

    private static AbsolutePath AnyTarFile =>
        ArtifactsDirectory / $"*.{TarFormat}";

    private AbsolutePath ZipFile =>
        GetArchiveFile(ZipFormat);

    private AbsolutePath TarFile =>
        GetArchiveFile(TarFormat);

    private AbsolutePath SourceLinuxInstallFile =>
        this.LinuxFilesDirectory / "install.sh";

    private AbsolutePath SourceLinuxUninstallFile =>
        this.LinuxFilesDirectory / "uninstall.sh";

    private AbsolutePath GetArchiveFile(string format) =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.Archive}.{format}";
}

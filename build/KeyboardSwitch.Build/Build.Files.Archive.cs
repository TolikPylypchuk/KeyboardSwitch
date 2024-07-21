public partial class Build
{
    private static AbsolutePath AnyZipFile =>
        ArtifactsDirectory / $"*.zip";

    private static AbsolutePath AnyTarFile =>
        ArtifactsDirectory / $"*.tar.gz";

    private AbsolutePath ZipFile =>
        GetArchiveFile("zip", lowercase: false);

    private AbsolutePath TarFile =>
        GetArchiveFile("tar.gz", lowercase: true);

    private AbsolutePath SourceLinuxInstallFile =>
        this.LinuxFilesDirectory / "install.sh";

    private AbsolutePath SourceLinuxUninstallFile =>
        this.LinuxFilesDirectory / "uninstall.sh";

    private AbsolutePath GetArchiveFile(string format, bool lowercase) =>
        this.WithSuffix(
            ArtifactsDirectory /
                $"{(lowercase ? KeyboardSwitchLower : KeyboardSwitch)}-{Version}-{this.Platform.Archive}.{format}");
}

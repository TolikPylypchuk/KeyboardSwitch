using Nuke.Common.IO;

public partial class Build
{
    private static readonly string LinuxInstallFile = "install.sh";
    private static readonly string LinuxUninstallFile = "uninstall.sh";

    private AbsolutePath ZipFile =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.ZipPart}.zip";

    private AbsolutePath TarFile =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.TarPart}.tar.gz";

    private AbsolutePath SourceLinuxInstallFile =>
        this.LinuxFilesDirectory / LinuxInstallFile;

    private AbsolutePath TargetLinuxInstallFile =>
        PublishOutputDirectory / LinuxInstallFile;

    private AbsolutePath SourceLinuxUninstallFile =>
        this.LinuxFilesDirectory / LinuxUninstallFile;

    private AbsolutePath TargetLinuxUninstallFile =>
        PublishOutputDirectory / LinuxUninstallFile;
}

using Nuke.Common.IO;

public partial class Build
{
    private static readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";
    private static readonly AbsolutePath PublishOutputDirectory = ArtifactsDirectory / "publish";

    private static readonly AbsolutePath PngIcon = PublishOutputDirectory / "icon.png";
    private static readonly AbsolutePath AppleIcon = PublishOutputDirectory / "KeyboardSwitch.icns";

    private static readonly AbsolutePath AppSettingsWindows = PublishOutputDirectory / "appsettings.windows.json";
    private static readonly AbsolutePath AppSettingsMacOS = PublishOutputDirectory / "appsettings.macos.json";
    private static readonly AbsolutePath AppSettingsLinux = PublishOutputDirectory / "appsettings.linux.json";

    private static readonly string AppSettings = "appsettings.json";

    private static readonly string LinuxInstallFile = "install.sh";
    private static readonly string LinuxUninstallFile = "uninstall.sh";

    private AbsolutePath ZipFile =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.ZipPart}.zip";

    private AbsolutePath TarFile =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.TarPart}.tar.gz";

    private AbsolutePath BuildDirectory =>
        this.Solution.KeyboardSwitch_Build.Directory;

    private AbsolutePath LinuxFilesDirectory =>
        this.BuildDirectory / "linux";

    private AbsolutePath SourceLinuxInstallFile =>
        this.LinuxFilesDirectory / LinuxInstallFile;

    private AbsolutePath TargetLinuxInstallFile =>
        PublishOutputDirectory / LinuxInstallFile;

    private AbsolutePath SourceLinuxUninstallFile =>
        this.LinuxFilesDirectory / LinuxUninstallFile;

    private AbsolutePath TargetLinuxUninstallFile =>
        PublishOutputDirectory / LinuxUninstallFile;
}

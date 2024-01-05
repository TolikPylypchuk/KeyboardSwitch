public partial class Build
{
    private const string KeyboardSwitch = nameof(KeyboardSwitch);
    private const string KeyboardSwitchLower = "keyboard-switch";
    private const string KeyboardSwitchSettings = nameof(KeyboardSwitchSettings);

    private const string LicenseFile = "LICENSE";
    private const string AppSettings = "appsettings.json";
    private const string LinuxIconFile = $"{KeyboardSwitchLower}.png";

    private static readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";
    private static readonly AbsolutePath PublishOutputDirectory = ArtifactsDirectory / "publish";

    private static AbsolutePath SourceLicenseFile =>
        RootDirectory / LicenseFile;

    private static AbsolutePath AppSettingsWindows =>
        PublishOutputDirectory / "appsettings.windows.json";

    private static AbsolutePath AppSettingsMacOS =>
        PublishOutputDirectory / "appsettings.macos.json";

    private static AbsolutePath AppSettingsLinux =>
        PublishOutputDirectory / "appsettings.linux.json";

    private AbsolutePath BuildDirectory =>
        this.Solution.KeyboardSwitch_Build.Directory;

    private AbsolutePath MacOSFilesDirectory =>
        this.BuildDirectory / "macos";

    private AbsolutePath LinuxFilesDirectory =>
        this.BuildDirectory / "linux";

    private AbsolutePath SourceAppleIconFile =>
        this.MacOSFilesDirectory / $"{KeyboardSwitch}.icns";

    private AbsolutePath SourceLinuxIconFile =>
        this.LinuxFilesDirectory / LinuxIconFile;
}

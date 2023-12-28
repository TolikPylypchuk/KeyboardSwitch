using Nuke.Common.IO;

public partial class Build
{
    private static readonly string KeyboardSwitch = nameof(KeyboardSwitch);
    private static readonly string KeyboardSwitchLower = "keyboard-switch";

    private static readonly AbsolutePath ArtifactsDirectory = RootDirectory / "artifacts";
    private static readonly AbsolutePath PublishOutputDirectory = ArtifactsDirectory / "publish";

    private static readonly AbsolutePath AppSettingsWindows = PublishOutputDirectory / "appsettings.windows.json";
    private static readonly AbsolutePath AppSettingsMacOS = PublishOutputDirectory / "appsettings.macos.json";
    private static readonly AbsolutePath AppSettingsLinux = PublishOutputDirectory / "appsettings.linux.json";

    private static readonly string AppSettings = "appsettings.json";

    private AbsolutePath BuildDirectory =>
        this.Solution.KeyboardSwitch_Build.Directory;

    private AbsolutePath LinuxFilesDirectory =>
        this.BuildDirectory / "linux";
}

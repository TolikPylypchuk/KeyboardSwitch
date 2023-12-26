using Nuke.Common.IO;

public partial class Build
{
    private static readonly AbsolutePath ArtifactsPath = RootDirectory / "artifacts";
    private static readonly AbsolutePath PublishOutputPath = ArtifactsPath / "publish";

    private static readonly AbsolutePath PngIcon = PublishOutputPath / "icon.png";
    private static readonly AbsolutePath AppleIcon = PublishOutputPath / "KeyboardSwitch.icns";

    private static readonly AbsolutePath AppSettingsWindows = PublishOutputPath / "appsettings.windows.json";
    private static readonly AbsolutePath AppSettingsMacOS = PublishOutputPath / "appsettings.macos.json";
    private static readonly AbsolutePath AppSettingsLinux = PublishOutputPath / "appsettings.linux.json";

    private static readonly string AppSettings = "appsettings.json";

    private AbsolutePath ZipFile =>
        ArtifactsPath / $"{KeyboardSwitch}-{Version}-{this.Platform.ZipPart}.zip";
}

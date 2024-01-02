public partial class Build
{
    private static readonly string PkgEntitlementsFile = "entitlements.plist";
    private static readonly string PkgDistributionFile = "dist-pkg.xml";
    private static readonly string AppInfoFile = "Info.plist";

    private static readonly AbsolutePath PkgScriptsDirectory = ArtifactsDirectory / "scripts";
    private static readonly AbsolutePath PkgResourcesDirectory = ArtifactsDirectory / "resources";

    private static readonly AbsolutePath KeyboardSwitchAppDirectory =
        ArtifactsDirectory / "Keyboard Switch.app";

    private static readonly AbsolutePath KeyboardSwitchAppContentsDirectory =
        KeyboardSwitchAppDirectory / "Contents";

    private static readonly AbsolutePath KeyboardSwitchAppMacOSDirectory =
        KeyboardSwitchAppContentsDirectory / "MacOS";

    private static readonly AbsolutePath KeyboardSwitchAppResourcesDirectory =
        KeyboardSwitchAppContentsDirectory / "Resources";

    private static readonly AbsolutePath KeyboardSwitchSettingsAppDirectory =
        ArtifactsDirectory / "Keyboard Switch Settings.app";

    private static readonly AbsolutePath KeyboardSwitchSettingsAppContentsDirectory =
        KeyboardSwitchSettingsAppDirectory / "Contents";

    private static readonly AbsolutePath KeyboardSwitchSettingsAppMacOSDirectory =
        KeyboardSwitchSettingsAppContentsDirectory / "MacOS";

    private static readonly AbsolutePath KeyboardSwitchSettingsAppResourcesDirectory =
        KeyboardSwitchSettingsAppContentsDirectory / "Resources";

    private static readonly AbsolutePath KeyboardSwitchExecutableFile =
        PublishOutputDirectory / KeyboardSwitch;

    private static readonly AbsolutePath KeyboardSwitchSettingsExecutableFile =
        PublishOutputDirectory / KeyboardSwitchSettings;

    private static readonly AbsolutePath LibSqLiteFile =
        PublishOutputDirectory / "libe_sqlite3.dylib";

    private static readonly AbsolutePath LibUioHookFile =
        PublishOutputDirectory / "libuiohook.dylib";

    private static readonly AbsolutePath LibAvaloniaNativeFile =
        PublishOutputDirectory / "libAvaloniaNative.dylib";

    private static readonly AbsolutePath LibHarfBuzzSharpFile =
        PublishOutputDirectory / "libHarfBuzzSharp.dylib";

    private static readonly AbsolutePath LibSkiaSharpFile =
        PublishOutputDirectory / "libSkiaSharp.dylib";

    private string PkgFileName =>
        $"{KeyboardSwitch}-{Version}-{this.Platform.Pkg}.pkg";

    private AbsolutePath PkgFile =>
        ArtifactsDirectory / PkgFileName;

    private AbsolutePath SourcePkgEntitlementsFile =>
        this.MacOSFilesDirectory / PkgEntitlementsFile;

    private AbsolutePath TargetPkgEntitlementsFile =>
        PublishOutputDirectory / PkgEntitlementsFile;

    private AbsolutePath SourcePkgDistributionFile =>
        this.MacOSFilesDirectory / PkgDistributionFile;

    private AbsolutePath TargetPkgDistributionFile =>
        PublishOutputDirectory / PkgDistributionFile;

    private AbsolutePath SourcePkgPostInstallFile =>
        this.MacOSFilesDirectory / "postinstall-pkg";

    private AbsolutePath TargetPkgPostInstallFile =>
        PkgScriptsDirectory / "postinstall";

    private AbsolutePath SourcePkgReadmeFile =>
        this.MacOSFilesDirectory / "readme.txt";

    private AbsolutePath SourcePkgLicenseFile =>
        this.MacOSFilesDirectory / "license.txt";

    private AbsolutePath SourceKeyboardSwitchAppInfoFile =>
        this.MacOSFilesDirectory / $"{KeyboardSwitch}.plist";

    private AbsolutePath TargetKeyboardSwitchAppInfoFile =>
        KeyboardSwitchAppContentsDirectory / AppInfoFile;

    private AbsolutePath SourceKeyboardSwitchSettingsAppInfoFile =>
        this.MacOSFilesDirectory / $"{KeyboardSwitchSettings}.plist";

    private AbsolutePath TargetKeyboardSwitchSettingsAppInfoFile =>
        KeyboardSwitchSettingsAppContentsDirectory / AppInfoFile;

    private AbsolutePath SourceKeyboardSwitchServiceInfoFile =>
        this.MacOSFilesDirectory / "io.tolik.keyboardswitch.plist";
}

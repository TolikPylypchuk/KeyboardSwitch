public partial class Build
{
    private const string PkgEntitlementsFile = "entitlements.plist";
    private const string PkgDistributionFile = "dist-pkg.xml";
    private const string UninstallerPkgDistributionFile = "dist-uninstaller-pkg.xml";
    private const string AppInfoFile = "Info.plist";
    private const string KeyboardSwitchUninstaller = nameof(KeyboardSwitchUninstaller);

    private const string LibSqLite = "libe_sqlite3.dylib";
    private const string LibUioHook = "libuiohook.dylib";
    private const string LibAvaloniaNative = "libAvaloniaNative.dylib";
    private const string LibHarfBuzzSharp = "libHarfBuzzSharp.dylib";
    private const string LibSkiaSharp = "libSkiaSharp.dylib";

    private static AbsolutePath PkgScriptsDirectory =>
        ArtifactsDirectory / "scripts";

    private static AbsolutePath PkgResourcesDirectory =>
        ArtifactsDirectory / "resources";

    private static AbsolutePath KeyboardSwitchAppDirectory =>
        ArtifactsDirectory / "Keyboard Switch.app";

    private static AbsolutePath KeyboardSwitchAppContentsDirectory =>
        KeyboardSwitchAppDirectory / "Contents";

    private static AbsolutePath KeyboardSwitchAppMacOSDirectory =>
        KeyboardSwitchAppContentsDirectory / "MacOS";

    private static AbsolutePath KeyboardSwitchAppResourcesDirectory =>
        KeyboardSwitchAppContentsDirectory / "Resources";

    private static AbsolutePath KeyboardSwitchSettingsAppDirectory =>
        ArtifactsDirectory / "Keyboard Switch Settings.app";

    private static AbsolutePath KeyboardSwitchSettingsAppContentsDirectory =>
        KeyboardSwitchSettingsAppDirectory / "Contents";

    private static AbsolutePath KeyboardSwitchSettingsAppMacOSDirectory =>
        KeyboardSwitchSettingsAppContentsDirectory / "MacOS";

    private static AbsolutePath KeyboardSwitchSettingsAppResourcesDirectory =>
        KeyboardSwitchSettingsAppContentsDirectory / "Resources";

    private static AbsolutePath KeyboardSwitchExecutableFile =>
        PublishOutputDirectory / KeyboardSwitch;

    private static AbsolutePath KeyboardSwitchSettingsExecutableFile =>
        PublishOutputDirectory / KeyboardSwitchSettings;

    private static AbsolutePath LibSqLiteFile =>
        PublishOutputDirectory / LibSqLite;

    private static AbsolutePath LibUioHookFile =>
        PublishOutputDirectory / LibUioHook;

    private static AbsolutePath LibAvaloniaNativeFile =>
        PublishOutputDirectory / LibAvaloniaNative;

    private static AbsolutePath LibHarfBuzzSharpFile =>
        PublishOutputDirectory / LibHarfBuzzSharp;

    private static AbsolutePath LibSkiaSharpFile =>
        PublishOutputDirectory / LibSkiaSharp;

    private static AbsolutePath KeyboardSwitchAppExecutableFile =>
        KeyboardSwitchAppMacOSDirectory / KeyboardSwitch;

    private static AbsolutePath KeyboardSwitchAppLibSqLiteFile =>
        KeyboardSwitchAppMacOSDirectory / LibSqLite;

    private static AbsolutePath KeyboardSwitchAppLibUioHookFile =>
        KeyboardSwitchAppMacOSDirectory / LibUioHook;

    private static AbsolutePath KeyboardSwitchSettingsAppExecutableFile =>
        KeyboardSwitchSettingsAppMacOSDirectory / KeyboardSwitchSettings;

    private static AbsolutePath KeyboardSwitchSettingsAppLibSqLiteFile =>
        KeyboardSwitchSettingsAppMacOSDirectory / LibSqLite;

    private static AbsolutePath KeyboardSwitchSettingsAppLibAvaloniaNativeFile =>
        KeyboardSwitchSettingsAppMacOSDirectory / LibAvaloniaNative;

    private static AbsolutePath KeyboardSwitchSettingsAppLibHarfBuzzSharpFile =>
        KeyboardSwitchSettingsAppMacOSDirectory / LibHarfBuzzSharp;

    private static AbsolutePath KeyboardSwitchSettingsAppLibSkiaSharpFile =>
        KeyboardSwitchSettingsAppMacOSDirectory / LibSkiaSharp;

    private static AbsolutePath KeyboardSwitchPkgFile =>
        ArtifactsDirectory / $"{KeyboardSwitch}.pkg";

    private static AbsolutePath KeyboardSwitchSettingsPkgFile =>
        ArtifactsDirectory / $"{KeyboardSwitchSettings}.pkg";

    private static AbsolutePath KeyboardSwitchUninstallerPkgFile =>
        ArtifactsDirectory / $"{KeyboardSwitchUninstaller}.pkg";

    private AbsolutePath PkgFile =>
        ArtifactsDirectory / $"{KeyboardSwitch}-{Version}-{this.Platform.Pkg}.pkg";

    private AbsolutePath UninstallerPkgFile =>
        ArtifactsDirectory / $"{KeyboardSwitchUninstaller}-{Version}.pkg";

    private AbsolutePath SourcePkgEntitlementsFile =>
        this.MacOSFilesDirectory / PkgEntitlementsFile;

    private AbsolutePath TargetPkgEntitlementsFile =>
        ArtifactsDirectory / PkgEntitlementsFile;

    private AbsolutePath SourcePkgDistributionFile =>
        this.MacOSFilesDirectory / PkgDistributionFile;

    private AbsolutePath TargetPkgDistributionFile =>
        ArtifactsDirectory / PkgDistributionFile;

    private AbsolutePath SourceUninstallerPkgDistributionFile =>
        this.MacOSFilesDirectory / UninstallerPkgDistributionFile;

    private AbsolutePath TargetUninstallerPkgDistributionFile =>
        ArtifactsDirectory / UninstallerPkgDistributionFile;

    private AbsolutePath SourcePkgPostInstallFile =>
        this.MacOSFilesDirectory / "postinstall-pkg";

    private AbsolutePath SourceUninstallerPkgPostInstallFile =>
        this.MacOSFilesDirectory / "postinstall-uninstaller-pkg";

    private AbsolutePath TargetPkgPostInstallFile =>
        PkgScriptsDirectory / "postinstall";

    private AbsolutePath SourcePkgReadmeFile =>
        this.MacOSFilesDirectory / "readme.txt";

    private AbsolutePath SourcePkgLicenseFile =>
        this.MacOSFilesDirectory / "license.txt";

    private AbsolutePath SourceUninstallerPkgWelcomeFile =>
        this.MacOSFilesDirectory / "welcome.txt";

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

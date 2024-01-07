using System.Runtime.InteropServices;

public partial class Build
{
    private const string DefaultNotaryToolKeychainProfile = "notarytool-password";

    [Parameter("Configuration - Release by default")]
    public readonly Configuration Configuration = Configuration.Release;

    [Parameter("Target OS - current OS by default")]
    public readonly TargetOS TargetOS =
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? TargetOS.MacOS
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? TargetOS.Linux
                : TargetOS.Windows;

    [Parameter("Platform - current architecture by default")]
    public readonly Platform Platform =
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? Platform.Arm64 : Platform.X64;

    [Parameter("Publish single file - false by default")]
    public readonly bool PublishSingleFile;

    [Parameter("Archive format - zip by default")]
    public readonly ArchiveFormat ArchiveFormat = ArchiveFormat.Zip;

    [Parameter("Apple ID")]
    public readonly string? AppleId;

    [Parameter("Apple team ID")]
    public readonly string? AppleTeamId;

    [Parameter("Apple application certificate")]
    public readonly string? AppleApplicationCertificate;

    [Parameter("Apple installer certificate")]
    public readonly string? AppleInstallerCertificate;

    [Parameter($"Keychain profile for the Apple notary tool - '{DefaultNotaryToolKeychainProfile}' by default")]
    public readonly string NotaryToolKeychainProfile = DefaultNotaryToolKeychainProfile;

    private string RuntimeIdentifier =>
        $"{this.TargetOS.RuntimeIdentifierPart}-{this.Platform.RuntimeIdentifierPart}";

    private bool IsSelfContained =>
        this.PublishSingleFile || this.Configuration == Configuration.Release;
}

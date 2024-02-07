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

    [Parameter("Publish single file - true by default")]
    public readonly bool PublishSingleFile = true;

    [Parameter("Output file suffix")]
    public readonly string? OutputFileSuffix;

    [Parameter("Apple ID")]
    public readonly string? AppleId;

    [Parameter("Apple team ID")]
    public readonly string? AppleTeamId;

    [Parameter("Apple application certificate")]
    public readonly string? AppleApplicationCertificate;

    [Secret, Parameter("Apple application certificate password")]
    public readonly string? AppleApplicationCertificatePassword;

    [Secret, Parameter("Apple application certificate value in base64")]
    public readonly string? AppleApplicationCertificateValue;

    [Parameter("Apple installer certificate")]
    public readonly string? AppleInstallerCertificate;

    [Secret, Parameter("Apple installer certificate password")]
    public readonly string? AppleInstallerCertificatePassword;

    [Secret, Parameter("Apple installer certificate value in base64")]
    public readonly string? AppleInstallerCertificateValue;

    [Secret, Parameter($"Keychain password")]
    public readonly string? KeychainPassword;

    [Secret, Parameter($"Password for the Apple notary tool")]
    public readonly string? NotarizationPassword;

    [Parameter($"Keychain profile for the Apple notary tool - '{DefaultNotaryToolKeychainProfile}' by default")]
    public readonly string NotaryToolKeychainProfile = DefaultNotaryToolKeychainProfile;

    private string RuntimeIdentifier =>
        $"{this.TargetOS.RuntimeIdentifierPart}-{this.Platform.RuntimeIdentifierPart}";

    private bool IsSelfContained =>
        this.PublishSingleFile || this.Configuration == Configuration.Release;
}

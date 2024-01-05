using System.Runtime.InteropServices;

public partial class Build
{
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

    [Secret]
    [Parameter("Apple ID")]
    public readonly string? AppleId;

    [Secret]
    [Parameter("Apple team ID")]
    public readonly string? AppleTeamId;

    [Secret]
    [Parameter("Apple application certificate")]
    public readonly string? AppleApplicationCertificate;

    [Secret]
    [Parameter("Apple installer certificate")]
    public readonly string? AppleInstallerCertificate;

    [Secret]
    [Parameter("Password for the Apple notarization service")]
    public readonly string? NotarizationPassword;

    private string RuntimeIdentifier =>
        $"{this.TargetOS.RuntimeIdentifierPart}-{this.Platform.RuntimeIdentifierPart}";

    private bool IsSelfContained =>
        this.PublishSingleFile || this.Configuration == Configuration.Release;
}

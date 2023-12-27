using System.Runtime.InteropServices;

public partial class Build
{
    [Parameter("Configuration - Release by default")]
    private readonly Configuration Configuration = Configuration.Release;

    [Parameter("Target OS - current OS by default")]
    private readonly TargetOS TargetOS =
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? TargetOS.MacOS
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? TargetOS.Linux
                : TargetOS.Windows;

    [Parameter("Platform - current architecture by default")]
    private readonly Platform Platform =
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? Platform.Arm64 : Platform.X64;

    [Parameter("Publish single file - false by default")]
    private readonly bool PublishSingleFile = false;

    [Parameter("Archive format - zip by default")]
    private readonly ArchiveFormat ArchiveFormat = ArchiveFormat.Zip;

    private string RuntimeIdentifer =>
        $"{this.TargetOS.RuntimeIdentifierPart}-{this.Platform.RuntimeIdentifierPart}";

    private bool IsSelfContained =>
        this.PublishSingleFile || this.Configuration == Configuration.Release;
}

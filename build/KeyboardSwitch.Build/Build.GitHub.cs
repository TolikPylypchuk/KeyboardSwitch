using Nuke.Common.CI.GitHubActions;

[MultipleGitHubActions(
    "build",
    "Build Keyboard Switch",
    typeof(Build),
    OnPushBranches = ["main"],
    OnWorkflowDispatch = true)]

[GitHubAction(
    "build-zip",
    "Build Zip Archive for Windows",
    GitHubActionsImage.WindowsLatest,
    InvokedTargets = [nameof(CreateZipArchive)],
    Parameters = [nameof(NukePlatform), PlatformValue, nameof(OutputFileSuffix), WindowsOutputFileSuffix],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"],
    ArtifactSuffix = PlatformValue,
    TimeoutMinutes = GitHubActionsTimeout)]

[GitHubAction(
    "build-msi",
    "Build Windows Installer",
    GitHubActionsImage.WindowsLatest,
    InvokedTargets = [nameof(CreateWindowsInstaller)],
    Parameters = [nameof(NukePlatform), PlatformValue, nameof(OutputFileSuffix), WindowsOutputFileSuffix],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"],
    ArtifactSuffix = PlatformValue,
    TimeoutMinutes = GitHubActionsTimeout)]

[GitHubAction(
    "build-pkg",
    "Build macOS Package",
    GitHubActionsImage.MacOsLatest,
    InvokedTargets = [nameof(CreateMacOSPackage)],
    Parameters = [nameof(NukePlatform), PlatformValue],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"],
    ImportSecrets =
    [
        nameof(AppleId),
        nameof(AppleTeamId),
        nameof(AppleApplicationCertificate),
        nameof(AppleApplicationCertificatePassword),
        nameof(AppleApplicationCertificateValue),
        nameof(AppleInstallerCertificate),
        nameof(AppleInstallerCertificatePassword),
        nameof(AppleInstallerCertificateValue),
        nameof(KeychainPassword),
        nameof(NotarizationPassword)
    ],
    ArtifactSuffix = PlatformValue,
    TimeoutMinutes = GitHubActionsTimeout)]

[GitHubAction(
    "build-uninstaller-pkg",
    "Build macOS Uninstaller Package",
    GitHubActionsImage.MacOsLatest,
    InvokedTargets = [nameof(CreateMacOSUninstallerPackage)],
    ImportSecrets =
    [
        nameof(AppleId),
        nameof(AppleTeamId),
        nameof(AppleApplicationCertificate),
        nameof(AppleApplicationCertificatePassword),
        nameof(AppleApplicationCertificateValue),
        nameof(AppleInstallerCertificate),
        nameof(AppleInstallerCertificatePassword),
        nameof(AppleInstallerCertificateValue),
        nameof(KeychainPassword),
        nameof(NotarizationPassword)
    ],
    ArtifactSuffix = PlatformValue,
    TimeoutMinutes = GitHubActionsTimeout)]

[GitHubAction(
    "build-tar",
    "Build Tar Archive for Linux",
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = [nameof(CreateTarArchive)],
    Parameters = [nameof(NukePlatform), PlatformValue],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"],
    ArtifactSuffix = PlatformValue,
    TimeoutMinutes = GitHubActionsTimeout)]

[GitHubAction(
    "build-deb",
    "Build Debian Package",
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = [nameof(CreateDebianPackage)],
    Parameters = [nameof(NukePlatform), PlatformValue],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"],
    ArtifactSuffix = PlatformValue,
    TimeoutMinutes = GitHubActionsTimeout)]

[GitHubAction(
    "build-rpm",
    "Build RPM Package",
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = [nameof(CreateRpmPackage)],
    Parameters = [nameof(NukePlatform), PlatformValue],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"],
    ArtifactSuffix = PlatformValue,
    TimeoutMinutes = GitHubActionsTimeout)]

public partial class Build
{
    // Using nameof(Platform) makes GitHub Actions fail the builds for arm64 -
    // apparently this env variable is used somewhere else as well - but NUKE allows prefixing env variables with 'Nuke'
    public const string NukePlatform = "Nuke" + nameof(Platform);
    public const string MatrixPlatform = "platform";
    public const string PlatformValue = $"${{{{ matrix.{MatrixPlatform} }}}}";
    public const int GitHubActionsTimeout = 30;
    public const string WindowsOutputFileSuffix = "win";
}

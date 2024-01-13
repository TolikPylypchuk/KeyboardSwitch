using Nuke.Common.CI.GitHubActions;

[MultipleGitHubActions(
    "build",
    "Build Keyboard Switch",
    typeof(Build),
    OnPushBranches = ["main"],
    OnWorkflowDispatch = true,
    CacheKeyFiles = [])]

[GitHubAction(
    "build-zip",
    "Build Zip Archive for Windows",
    GitHubActionsImage.WindowsLatest,
    InvokedTargets = [nameof(CreateZipArchive)],
    Parameters = [nameof(Platform), $"${{{{ matrix.{MatrixPlatform} }}}}"],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"])]

[GitHubAction(
    "build-pkg",
    "Build macOS Package",
    GitHubActionsImage.MacOsLatest,
    InvokedTargets = [nameof(CreateMacOSPackage)],
    Parameters =
    [
        nameof(Platform), $"${{{{ matrix.{MatrixPlatform} }}}}",
        nameof(PublishSingleFile), "true"
    ],
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
    ])]

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
    ])]

[GitHubAction(
    "build-tar",
    "Build Tar Archive for Linux",
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = [nameof(CreateTarArchive)],
    Parameters = [nameof(Platform), $"${{{{ matrix.{MatrixPlatform} }}}}"],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"])]

[GitHubAction(
    "build-deb",
    "Build Debian Package",
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = [nameof(CreateDebianPackage)],
    Parameters = [nameof(Platform), $"${{{{ matrix.{MatrixPlatform} }}}}"],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"])]

[GitHubAction(
    "build-rpm",
    "Build RPM Package",
    GitHubActionsImage.UbuntuLatest,
    InvokedTargets = [nameof(CreateRpmPackage)],
    Parameters = [nameof(Platform), $"${{{{ matrix.{MatrixPlatform} }}}}"],
    Matrix = [MatrixPlatform, $"[ {Platform.X64Value}, {Platform.Arm64Value} ]"])]

public partial class Build
{
    public const string MatrixPlatform = "platform";
}
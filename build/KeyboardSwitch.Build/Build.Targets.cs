using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    public Target Restore => t => t
        .Description("Restores the projects")
        .Executes(() =>
        {
            foreach (var project in this.GetProjects(includeTests: true, includeInstaller: true))
            {
                Log.Information("Restoring project {Name}", project.Name);
                DotNetRestore(s => s
                    .SetProjectFile(project)
                    .SetRuntime(this.RuntimeIdentifier)
                    .SetPlatform(this.Platform.MSBuild)
                    .SetProperty(nameof(TargetOS), this.TargetOS));
            }
        });

    public Target Clean => t => t
        .Description("Cleans the build outputs")
        .DependsOn(this.Restore)
        .Executes(() =>
        {
            foreach (var project in this.GetProjects(includeTests: true, includeInstaller: true))
            {
                Log.Information("Cleaning project {Name}", project.Name);
                DotNetClean(s => s
                    .SetProject(project)
                    .SetRuntime(this.RuntimeIdentifier)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform.MSBuild)
                    .SetProperty(nameof(TargetOS), this.TargetOS));
            }
        });

    public Target Compile => t => t
        .Description("Builds the project")
        .DependsOn(this.Clean)
        .Executes(() =>
        {
            foreach (var project in this.GetProjects())
            {
                Log.Information("Building project {Name}", project.Name);
                DotNetBuild(s => s
                    .SetProjectFile(project)
                    .SetRuntime(this.RuntimeIdentifier)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform.MSBuild)
                    .SetProperty(nameof(TargetOS), this.TargetOS)
                    .SetNoRestore(true)
                    .SetSelfContained(this.IsSelfContained)
                    .SetPublishSingleFile(this.PublishSingleFile)
                    .SetContinuousIntegrationBuild(true));
            }
        });

    public Target Test => t => t
        .Description("Tests the project")
        .DependsOn(this.Compile)
        .Executes(() =>
        {
            Log.Information("Running tests in the project {Name}", this.Solution.KeyboardSwitch_Tests.Name);

            DotNetTest(s => s
                .SetProjectFile(this.Solution.KeyboardSwitch_Tests)
                .SetConfiguration(this.Configuration)
                .SetNoRestore(true));
        });

    public Target Publish => t => t
        .Description("Publishes the project")
        .DependsOn(this.Test)
        .Requires(() => this.Configuration == Configuration.Release)
        .Executes(() =>
        {
            Log.Information("Cleaning the publish directory");
            PublishOutputDirectory.CreateOrCleanDirectory();

            Log.Information("Publishing projects");

            var projects = new[] { this.Solution.KeyboardSwitch, this.Solution.KeyboardSwitch_Settings };

            DotNetPublish(s => s
                .SetRuntime(this.RuntimeIdentifier)
                .SetConfiguration(this.Configuration)
                .SetPlatform(this.Platform.MSBuild)
                .SetProperty(nameof(TargetOS), this.TargetOS)
                .SetNoBuild(true)
                .SetNoRestore(true)
                .SetOutput(PublishOutputDirectory)
                .SetSelfContained(this.IsSelfContained)
                .SetPublishSingleFile(this.PublishSingleFile)
                .CombineWith(projects, (ps, project) => ps.SetProject(project)));

            Log.Information("Deleting unneeded files after publish");

            var appSettingsFile = this.PlatformDependent(
                windows: AppSettingsWindows,
                macos: AppSettingsMacOS,
                linux: AppSettingsLinux);

            appSettingsFile.Rename(AppSettings);

            AppSettingsWindows.DeleteFile();
            AppSettingsMacOS.DeleteFile();
            AppSettingsLinux.DeleteFile();
        });

    public Target PreCreateArchive => t => t
        .Description("Copies additional files to the publish directory")
        .DependentFor(this.CreateZipArchive, this.CreateTarArchive)
        .OnlyWhenStatic(() => this.TargetOS == TargetOS.Linux)
        .After(this.Publish)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Copying additional files to the publish directory");

            this.SourceLinuxInstallFile.CopyToDirectory(PublishOutputDirectory);
            this.SourceLinuxUninstallFile.CopyToDirectory(PublishOutputDirectory);
            SourceLinuxIconFile.CopyToDirectory(PublishOutputDirectory, ExistsPolicy.FileOverwrite);
        });

    public Target CreateZipArchive => t => t
        .Description("Creates a zip archive containing the published project")
        .DependsOn(this.Publish)
        .Produces(AnyZipFile)
        .Executes(() =>
        {
            Log.Information("Archiving the publish output into {ZipFile}", this.ZipFile);
            this.ZipFile.DeleteFile();
            PublishOutputDirectory.ZipTo(this.ZipFile);
        });

    public Target CreateTarArchive => t => t
        .Description("Creates a tar archive containing the published project")
        .DependsOn(this.Publish)
        .Produces(AnyTarFile)
        .Executes(() =>
        {
            Log.Information("Archiving the publish output into {TarFile}", this.ZipFile);
            this.TarFile.DeleteFile();
            PublishOutputDirectory.TarGZipTo(this.TarFile);
        });

    public Target CreateWindowsInstaller => t => t
        .Description("Creates a Windows installer which installs the published project")
        .DependsOn(this.Publish)
        .Requires(() => this.TargetOS == TargetOS.Windows, () => this.PublishSingleFile)
        .Produces(AnyMsiFile)
        .Executes(() =>
        {
            Log.Information("Creating a Windows installer");

            DotNetBuild(s => s
                .SetProjectFile(this.Solution.KeyboardSwitch_Windows_Installer)
                .SetRuntime(this.RuntimeIdentifier)
                .SetConfiguration(this.Configuration)
                .SetPlatform(this.Platform.MSBuild)
                .SetProperty(nameof(TargetOS), this.TargetOS)
                .SetNoRestore(true)
                .SetSelfContained(this.IsSelfContained)
                .SetPublishSingleFile(this.PublishSingleFile)
                .SetContinuousIntegrationBuild(true));

            this.MsiFile.DeleteFile();
            this.SourceMsiFile.Move(this.MsiFile);
        });

    public Target SetupKeychain => t => t
        .Description("Sets up a macOS keychain and stores certificates in it")
        .DependentFor(this.CreateMacOSPackage, this.CreateMacOSUninstallerPackage)
        .After(this.PrepareMacOSPackage, this.PrepareMacOSUninstallerPackage)
        .OnlyWhenStatic(() => IsServerBuild)
        .Requires(() => this.security, () => this.xCodeRun)
        .Requires(
            () => this.AppleId,
            () => this.AppleTeamId,
            () => this.AppleApplicationCertificatePassword,
            () => this.AppleApplicationCertificateValue,
            () => this.AppleInstallerCertificatePassword,
            () => this.AppleInstallerCertificateValue,
            () => this.KeychainPassword,
            () => this.NotarizationPassword,
            () => this.NotaryToolKeychainProfile)
        .Executes(() =>
        {
            Log.Information("Setting up a macOS keychain and storing certificates in it");

            var applicationCertificate = RootDirectory / "application_certificate.p12";
            var installerCertificate = RootDirectory / "installer_certificate.p12";
            var keychain = RootDirectory / "app-signing.keychain-db";

            applicationCertificate.WriteAllBytes(Convert.FromBase64String(
                this.AppleApplicationCertificateValue.NotNull("Apple application certificate value not provided")!));

            installerCertificate.WriteAllBytes(Convert.FromBase64String(
                this.AppleInstallerCertificateValue.NotNull("Apple installer certificate value not provided")!));

            this.Security($"create-keychain -p {this.KeychainPassword} {keychain}", logger: DebugOnly);
            this.Security($"set-keychain-settings -lut 21600 {keychain}", logger: DebugOnly);
            this.Security($"unlock-keychain -p {this.KeychainPassword} {keychain}", logger: DebugOnly);

            this.Security(
                $"import {applicationCertificate} -P {this.AppleApplicationCertificatePassword} " +
                $"-A -t cert -f pkcs12 -k {keychain}",
                logger: DebugOnly);

            this.Security(
                $"import {installerCertificate} -P {this.AppleInstallerCertificatePassword} " +
                $"-A -t cert -f pkcs12 -k {keychain}",
                logger: DebugOnly);

            this.Security($"list-keychain -d user -s {keychain}", logger: DebugOnly);

            this.XCodeRun(
                $"notarytool store-credentials {this.NotaryToolKeychainProfile} --apple-id {this.AppleId} " +
                $"--team-id {this.AppleTeamId} --password {this.NotarizationPassword}",
                logger: DebugOnly);
        });

    public Target PrepareMacOSPackage => t => t
        .Description("Prepares files for creating a macOS package containing the published project")
        .DependsOn(this.Publish)
        .Requires(() => this.TargetOS == TargetOS.MacOS, () => this.PublishSingleFile)
        .Executes(() =>
        {
            Log.Information("Preparing files for creating a macOS package containing the published project");

            SourcePkgEntitlementsFile.Copy(TargetPkgEntitlementsFile, ExistsPolicy.FileOverwrite);
            SourcePkgDistributionFile.Copy(TargetPkgDistributionFile, ExistsPolicy.FileOverwrite);

            ResolvePlaceholders(this.TargetPkgDistributionFile, this.Platform.Pkg);

            PkgScriptsDirectory.CreateOrCleanDirectory();
            this.SourcePkgPreInstallFile.Copy(this.TargetPkgPreInstallFile);
            this.SourcePkgPostInstallFile.Copy(this.TargetPkgPostInstallFile);

            PkgResourcesDirectory.CreateOrCleanDirectory();
            this.SourcePkgReadmeFile.CopyToDirectory(PkgResourcesDirectory);
            this.SourcePkgLicenseFile.CopyToDirectory(PkgResourcesDirectory);

            KeyboardSwitchAppDirectory.CreateOrCleanDirectory();
            KeyboardSwitchAppMacOSDirectory.CreateOrCleanDirectory();

            KeyboardSwitchExecutableFile.CopyToDirectory(KeyboardSwitchAppMacOSDirectory);
            LibUioHookFile.CopyToDirectory(KeyboardSwitchAppMacOSDirectory);

            KeyboardSwitchAppResourcesDirectory.CreateOrCleanDirectory();

            var appSettings = PublishOutputDirectory / AppSettings;

            appSettings.CopyToDirectory(KeyboardSwitchAppResourcesDirectory);
            this.SourceAppleIconFile.CopyToDirectory(KeyboardSwitchAppResourcesDirectory);
            this.SourceKeyboardSwitchServiceInfoFile.CopyToDirectory(KeyboardSwitchAppResourcesDirectory);

            this.SourceKeyboardSwitchAppInfoFile.Copy(this.TargetKeyboardSwitchAppInfoFile);

            ResolvePlaceholders(this.TargetKeyboardSwitchAppInfoFile, this.Platform.Pkg);

            KeyboardSwitchSettingsAppDirectory.CreateOrCleanDirectory();
            KeyboardSwitchSettingsAppMacOSDirectory.CreateOrCleanDirectory();

            KeyboardSwitchSettingsExecutableFile.CopyToDirectory(KeyboardSwitchSettingsAppMacOSDirectory);
            LibAvaloniaNativeFile.CopyToDirectory(KeyboardSwitchSettingsAppMacOSDirectory);
            LibHarfBuzzSharpFile.CopyToDirectory(KeyboardSwitchSettingsAppMacOSDirectory);
            LibSkiaSharpFile.CopyToDirectory(KeyboardSwitchSettingsAppMacOSDirectory);

            KeyboardSwitchSettingsAppResourcesDirectory.CreateOrCleanDirectory();

            appSettings.CopyToDirectory(KeyboardSwitchSettingsAppResourcesDirectory);
            this.SourceAppleIconFile.CopyToDirectory(KeyboardSwitchSettingsAppResourcesDirectory);

            this.SourceKeyboardSwitchSettingsAppInfoFile.Copy(this.TargetKeyboardSwitchSettingsAppInfoFile);

            ResolvePlaceholders(this.TargetKeyboardSwitchSettingsAppInfoFile, this.Platform.Pkg);
        });

    public Target CreateMacOSPackage => t => t
        .Description("Creates a macOS package containing the published project")
        .DependsOn(this.PrepareMacOSPackage)
        .Requires(() => OperatingSystem.IsMacOS(), () => this.TargetOS == TargetOS.MacOS, () => this.PublishSingleFile)
        .Requires(() => this.codeSign, () => this.pkgBuild, () => this.productBuild, () => this.xCodeRun)
        .Requires(
            () => this.AppleId,
            () => this.AppleTeamId,
            () => this.AppleApplicationCertificate,
            () => this.AppleInstallerCertificate,
            () => this.NotaryToolKeychainProfile)
        .Produces(AnyPkgFile)
        .Executes(() =>
        {
            Log.Information("Creating a macOS package containing the published project");

            this.Sign(KeyboardSwitchAppLibUioHookFile);
            this.Sign(KeyboardSwitchAppExecutableFile, hardenedRuntime: true);

            this.Sign(KeyboardSwitchSettingsAppLibAvaloniaNativeFile);
            this.Sign(KeyboardSwitchSettingsAppLibHarfBuzzSharpFile);
            this.Sign(KeyboardSwitchSettingsAppLibSkiaSharpFile);
            this.Sign(KeyboardSwitchSettingsAppExecutableFile, hardenedRuntime: true);

            this.PkgBuild(
                $"--component {KeyboardSwitchAppDirectory} --identifier io.tolik.keyboardswitch.service " +
                $"--version {Version} --install-location {ServiceInstallationDirectory} " +
                $"--scripts {PkgScriptsDirectory} {KeyboardSwitchPkgFile}",
                logger: DebugOnly);

            this.PkgBuild(
                $"--component {KeyboardSwitchSettingsAppDirectory} --identifier io.tolik.keyboardswitch.settings " +
                $"--version {Version} --install-location {SettingsInstallationDirectory} " +
                $"{KeyboardSwitchSettingsPkgFile}",
                logger: DebugOnly);

            this.ProductBuild(
                $"--sign {this.AppleInstallerCertificate} --distribution {this.TargetPkgDistributionFile} " +
                $"--resources {PkgResourcesDirectory} {PkgFile}",
                workingDirectory: ArtifactsDirectory,
                logger: DebugOnly);

            this.XCodeRun(
                $"notarytool submit {this.PkgFile} --wait --apple-id {this.AppleId} " +
                $"--team-id {this.AppleTeamId} --keychain-profile {this.NotaryToolKeychainProfile}",
                logger: DebugOnly);

            this.XCodeRun($"stapler staple {this.PkgFile}", logger: DebugOnly);
        });

    public Target PrepareMacOSUninstallerPackage => t => t
        .Description("Prepares files for creating a macOS uninstaller package")
        .Executes(() =>
        {
            Log.Information("Preparing files for creating a macOS uninstaller package");

            SourceUninstallerPkgDistributionFile.Copy(TargetUninstallerPkgDistributionFile, ExistsPolicy.FileOverwrite);

            ResolvePlaceholders(
                this.TargetUninstallerPkgDistributionFile, $"{Platform.X64.Pkg},{Platform.Arm64.Pkg}");

            PkgScriptsDirectory.CreateOrCleanDirectory();
            this.SourceUninstallerPkgPostInstallFile.Copy(this.TargetPkgPostInstallFile);

            PkgResourcesDirectory.CreateOrCleanDirectory();
            this.SourceUninstallerPkgWelcomeFile.CopyToDirectory(PkgResourcesDirectory);
        });

    public Target CreateMacOSUninstallerPackage => t => t
        .Description("Creates a macOS uninstaller package")
        .DependsOn(this.PrepareMacOSUninstallerPackage)
        .Requires(() => OperatingSystem.IsMacOS())
        .Requires(() => this.pkgBuild, () => this.productBuild, () => this.xCodeRun)
        .Requires(
            () => this.AppleId,
            () => this.AppleTeamId,
            () => this.AppleInstallerCertificate,
            () => this.NotaryToolKeychainProfile)
        .Produces(AnyPkgFile)
        .Executes(() =>
        {
            Log.Information("Creating a macOS uninstaller package");

            this.PkgBuild(
                $"--nopayload --identifier io.tolik.keyboardswitch.uninstaller --scripts {PkgScriptsDirectory} " +
                $"--version {Version} {KeyboardSwitchUninstallerPkgFile}",
                logger: DebugOnly);

            this.ProductBuild(
                $"--sign {this.AppleInstallerCertificate} --distribution {this.TargetUninstallerPkgDistributionFile} " +
                $"--resources {PkgResourcesDirectory} {UninstallerPkgFile}",
                workingDirectory: ArtifactsDirectory,
                logger: DebugOnly);

            this.XCodeRun(
                $"notarytool submit {UninstallerPkgFile} --wait --apple-id {this.AppleId} " +
                $"--team-id {this.AppleTeamId} --keychain-profile {this.NotaryToolKeychainProfile}",
                logger: DebugOnly);

            this.XCodeRun($"stapler staple {UninstallerPkgFile}", logger: DebugOnly);
        });

    public Target PrepareDebianPackage => t => t
        .Description("Prepares files for creating a Debian package containing the published project")
        .DependsOn(this.Publish)
        .Requires(() => this.TargetOS == TargetOS.Linux)
        .Executes(() =>
        {
            Log.Information("Preparing files for creating a Debian package containing the published project");

            SourceLinuxIconFile.CopyToDirectory(PublishOutputDirectory, ExistsPolicy.FileOverwrite);

            this.DebDirectory.CreateOrCleanDirectory();

            this.DebConfigDirectory.CreateDirectory();

            this.SourceDebControlFile.Copy(this.TargetDebControlFile);
            this.SourceDebPostInstallFile.Copy(this.TargetDebPostInstallFile);
            this.SourceDebPreRemoveFile.Copy(this.TargetDebPreRemoveFile);
            this.SourceDebPostRemoveFile.Copy(this.TargetDebPostRemoveFile);

            ResolvePlaceholders(this.TargetDebControlFile, this.Platform.Deb);

            const string readAndExecute = "555";

            this.TargetDebPostInstallFile.SetUnixPermissions(readAndExecute);
            this.TargetDebPreRemoveFile.SetUnixPermissions(readAndExecute);
            this.TargetDebPostRemoveFile.SetUnixPermissions(readAndExecute);

            PublishOutputDirectory.Copy(this.DebKeyboardSwitchDirectory, createDirectories: true);

            this.SourceDebCopyrightFile.CopyToDirectory(this.DebDocsDirectory, createDirectories: true);
        });

    public Target CreateDebianPackage => t => t
        .Description("Creates a Debian package containing the published project")
        .DependsOn(this.PrepareDebianPackage)
        .Requires(() => OperatingSystem.IsLinux(), () => this.TargetOS == TargetOS.Linux)
        .Requires(() => this.debianPackageTool)
        .Produces(AnyDebFile)
        .Executes(() =>
        {
            Log.Information("Creating a Debian package containing the published project");
            this.DebianPackageTool($"--build --root-owner-group \"{this.DebDirectory}\"", logger: DebugOnly);
        });

    public Target PrepareRpmPackage => t => t
        .Description("Prepares files for creating an RPM package containing the published project")
        .DependsOn(this.Publish)
        .Requires(() => this.TargetOS == TargetOS.Linux)
        .Executes(() =>
        {
            Log.Information("Preparing files for creating an RPM package containing the published project");

            RpmDirectory.CreateOrCleanDirectory();

            this.SourceRpmSpecFile.Copy(this.TargetRpmSpecFile, ExistsPolicy.FileOverwrite);
            SourceLicenseFile.Copy(TargetRpmLicenseFile, ExistsPolicy.FileOverwrite);
            SourceLinuxIconFile.CopyToDirectory(PublishOutputDirectory, ExistsPolicy.FileOverwrite);

            ResolvePlaceholders(this.TargetRpmSpecFile, this.Platform.Rpm);
        });

    public Target CreateRpmPackage => t => t
        .Description("Creates an RPM package containing the published project")
        .DependsOn(this.PrepareRpmPackage)
        .Requires(() => OperatingSystem.IsLinux(), () => this.TargetOS == TargetOS.Linux)
        .Requires(() => this.rpmBuild)
        .Produces(AnyRpmFile)
        .Executes(() =>
        {
            Log.Information("Creating an RPM package containing the published project");

            this.RpmBuild(
                $"-bb --build-in-place --define \"_topdir {RpmDirectory}\" " +
                $"--target {this.Platform.Rpm} \"{this.TargetRpmSpecFile}\"",
                logger: DebugOnly);

            this.RpmOutputFile.Copy(this.RpmFile, ExistsPolicy.FileOverwrite);
        });

    public Target CleanUp => t => t
        .Description("Deletes leftover files")
        .TriggeredBy(
            this.CreateZipArchive,
            this.CreateTarArchive,
            this.CreateWindowsInstaller,
            this.CreateMacOSPackage,
            this.CreateMacOSUninstallerPackage,
            this.CreateDebianPackage,
            this.CreateRpmPackage)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting leftover files");

            PublishOutputDirectory.DeleteDirectory();

            KeyboardSwitchPkgFile.DeleteFile();
            KeyboardSwitchSettingsPkgFile.DeleteFile();
            KeyboardSwitchUninstallerPkgFile.DeleteFile();
            PkgResourcesDirectory.DeleteDirectory();
            PkgScriptsDirectory.DeleteDirectory();
            KeyboardSwitchAppDirectory.DeleteDirectory();
            KeyboardSwitchSettingsAppDirectory.DeleteDirectory();
            TargetPkgDistributionFile.DeleteFile();
            TargetUninstallerPkgDistributionFile.DeleteFile();
            TargetPkgEntitlementsFile.DeleteFile();

            this.DebDirectory.DeleteDirectory();

            RpmDirectory.DeleteDirectory();
            TargetRpmLicenseFile.DeleteFile();
            this.TargetRpmSpecFile.DeleteFile();
        });
}

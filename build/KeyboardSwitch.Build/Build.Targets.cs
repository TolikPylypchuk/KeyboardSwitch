using Serilog;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

public partial class Build
{
    public Target Clean => t => t
        .Description("Cleans the build outputs")
        .Executes(() =>
        {
            foreach (var project in this.GetProjects(includeInstaller: true))
            {
                Log.Information("Cleaning project {Name}", project.Name);
                DotNetClean(s => s
                    .SetProject(project)
                    .SetRuntime(this.RuntimeIdentifer)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform)
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
                    .SetRuntime(this.RuntimeIdentifer)
                    .SetConfiguration(this.Configuration)
                    .SetPlatform(this.Platform)
                    .SetProperty(nameof(TargetOS), this.TargetOS)
                    .SetSelfContained(this.IsSelfContained)
                    .SetPublishSingleFile(this.PublishSingleFile)
                    .SetContinuousIntegrationBuild(true));
            }
        });

    public Target Publish => t => t
        .Description("Publishes the project")
        .DependsOn(this.Compile)
        .Requires(() => this.Configuration == Configuration.Release)
        .Executes(() =>
        {
            Log.Information("Cleaning the publish directory");
            PublishOutputDirectory.CreateOrCleanDirectory();

            this.PublishProject(this.Solution.KeyboardSwitch);
            this.PublishProject(this.Solution.KeyboardSwitch_Settings);

            Log.Information("Deleting unneeded files after publish");

            var appSettingsFile = this.PlatformDependent(
                windows: AppSettingsWindows,
                macos: AppSettingsMacOS,
                linux: AppSettingsLinux);

            appSettingsFile.Rename(AppSettings);

            foreach (var file in new[] { AppSettingsWindows, AppSettingsMacOS, AppSettingsLinux }
                .Where(f => f != appSettingsFile))
            {
                file.DeleteFile();
            }
        });

    public Target PreCreateArchive => t => t
        .Description("Copies additional files to the publish directory")
        .DependentFor(this.CreateArchive)
        .OnlyWhenStatic(() => this.TargetOS == TargetOS.Linux)
        .After(this.Publish)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Copying additional files to the publish directory");

            CopyFileToDirectory(this.SourceLinuxInstallFile, PublishOutputDirectory);
            CopyFileToDirectory(this.SourceLinuxUninstallFile, PublishOutputDirectory);
        });

    public Target CreateArchive => t => t
        .Description("Creates an archive file containing the published project")
        .DependsOn(this.Publish)
        .Produces(this.ArchiveFile)
        .Executes(() =>
        {
            Log.Information("Archiving the publish output into {ArchiveFile}", this.ArchiveFile);
            this.ArchiveFile.DeleteFile();
            PublishOutputDirectory.CompressTo(this.ArchiveFile);
        });

    public Target PrepareMacOSPackage => t => t
        .Description("Prepares files for creating a macOS package containing the published project")
        .DependsOn(this.Publish)
        .Requires(() => this.TargetOS == TargetOS.MacOS, () => this.PublishSingleFile)
        .Executes(() =>
        {
            Log.Information("Preparing files for creating a macOS package containing the published project");

            CopyFile(SourcePkgEntitlementsFile, TargetPkgEntitlementsFile, FileExistsPolicy.Overwrite);
            CopyFile(SourcePkgDistributionFile, TargetPkgDistributionFile, FileExistsPolicy.Overwrite);

            this.TargetPkgDistributionFile.UpdateText(text => text
                .Replace(VersionPlaceholder, Version)
                .Replace(ArchitecturePlaceholder, this.Platform.Pkg));

            PkgScriptsDirectory.CreateOrCleanDirectory();
            CopyFile(this.SourcePkgPostInstallFile, this.TargetPkgPostInstallFile);

            PkgResourcesDirectory.CreateOrCleanDirectory();
            CopyFileToDirectory(this.SourcePkgReadmeFile, PkgResourcesDirectory);
            CopyFileToDirectory(this.SourcePkgLicenseFile, PkgResourcesDirectory);

            KeyboardSwitchAppDirectory.CreateOrCleanDirectory();
            KeyboardSwitchAppMacOSDirectory.CreateOrCleanDirectory();

            CopyFileToDirectory(KeyboardSwitchExecutableFile, KeyboardSwitchAppMacOSDirectory);
            CopyFileToDirectory(LibSqLiteFile, KeyboardSwitchAppMacOSDirectory);
            CopyFileToDirectory(LibUioHookFile, KeyboardSwitchAppMacOSDirectory);

            KeyboardSwitchAppResourcesDirectory.CreateOrCleanDirectory();

            var appSettings = PublishOutputDirectory / AppSettings;

            CopyFileToDirectory(appSettings, KeyboardSwitchAppResourcesDirectory);
            CopyFileToDirectory(this.SourceAppleIconFile, KeyboardSwitchAppResourcesDirectory);
            CopyFileToDirectory(this.SourceKeyboardSwitchServiceInfoFile, KeyboardSwitchAppResourcesDirectory);

            CopyFile(this.SourceKeyboardSwitchAppInfoFile, this.TargetKeyboardSwitchAppInfoFile);

            ResolvePlaceholders(this.TargetKeyboardSwitchAppInfoFile, this.Platform.Pkg);

            KeyboardSwitchSettingsAppDirectory.CreateOrCleanDirectory();
            KeyboardSwitchSettingsAppMacOSDirectory.CreateOrCleanDirectory();

            CopyFileToDirectory(KeyboardSwitchSettingsExecutableFile, KeyboardSwitchSettingsAppMacOSDirectory);
            CopyFileToDirectory(LibAvaloniaNativeFile, KeyboardSwitchSettingsAppMacOSDirectory);
            CopyFileToDirectory(LibSqLiteFile, KeyboardSwitchSettingsAppMacOSDirectory);
            CopyFileToDirectory(LibHarfBuzzSharpFile, KeyboardSwitchSettingsAppMacOSDirectory);
            CopyFileToDirectory(LibSkiaSharpFile, KeyboardSwitchSettingsAppMacOSDirectory);

            KeyboardSwitchSettingsAppResourcesDirectory.CreateOrCleanDirectory();

            CopyFileToDirectory(appSettings, KeyboardSwitchSettingsAppResourcesDirectory);
            CopyFileToDirectory(this.SourceAppleIconFile, KeyboardSwitchSettingsAppResourcesDirectory);

            CopyFile(this.SourceKeyboardSwitchSettingsAppInfoFile, this.TargetKeyboardSwitchSettingsAppInfoFile);

            ResolvePlaceholders(this.TargetKeyboardSwitchSettingsAppInfoFile, this.Platform.Pkg);
        });

    public Target CreateMacOSPackage => t => t
        .Description("Creates a macOS package containing the published project")
        .DependsOn(this.PrepareMacOSPackage)
        .Requires(() => OperatingSystem.IsMacOS(), () => this.TargetOS == TargetOS.MacOS, () => this.PublishSingleFile)
        .Requires(() => this.CodeSign, () => this.PkgBuild, () => this.ProductBuild, () => this.XCodeRun)
        .Produces(this.PkgFile)
        .Executes(() =>
        {
            Assert.NotNull(this.CodeSign, "codesign is not available");
            Assert.NotNull(this.PkgBuild, "pkgbuild is not available");
            Assert.NotNull(this.ProductBuild, "productbuild is not available");
            Assert.NotNull(this.XCodeRun, "xcrun is not available");
        });

    public Target PrepareDebianPackage => t => t
        .Description("Prepares files for creating a Debian package containing the published project")
        .DependsOn(this.Publish)
        .Requires(() => this.TargetOS == TargetOS.Linux)
        .Executes(() =>
        {
            Log.Information("Preparing files for creating a Debian package containing the published project");

            this.DebDirectory.CreateOrCleanDirectory();

            this.DebConfigDirectory.CreateDirectory();

            CopyFile(this.SourceDebControlFile, this.TargetDebControlFile);
            CopyFile(this.SourceDebPostInstallFile, this.TargetDebPostInstallFile);
            CopyFile(this.SourceDebPreRemoveFile, this.TargetDebPreRemoveFile);
            CopyFile(this.SourceDebPostRemoveFile, this.TargetDebPostRemoveFile);

            ResolvePlaceholders(this.TargetDebControlFile, this.Platform.Deb);

            const string readAndExecute = "555";

            this.TargetDebPostInstallFile.SetUnixPermissions(readAndExecute);
            this.TargetDebPreRemoveFile.SetUnixPermissions(readAndExecute);
            this.TargetDebPostRemoveFile.SetUnixPermissions(readAndExecute);

            CopyDirectoryRecursively(PublishOutputDirectory, this.DebKeyboardSwitchDirectory);

            CopyFileToDirectory(this.SourceDebCopyrightFile, this.DebDocsDirectory, createDirectories: true);

            CopyFile(this.SourceLinuxIconFile, this.TargetDebIconFile);
            this.TargetDebIconFile.SetUnixPermissions("644");
        });

    public Target CreateDebianPackage => t => t
        .Description("Creates a Debian package containing the published project")
        .DependsOn(this.PrepareDebianPackage)
        .Requires(() => OperatingSystem.IsLinux(), () => this.TargetOS == TargetOS.Linux)
        .Requires(() => this.DebianPackageTool)
        .Produces(this.DebFile)
        .Executes(() =>
        {
            Assert.NotNull(this.DebianPackageTool, "dpkg-deb is not available");

            Log.Information("Creating a Debian package containing the published project");

            this.DebianPackageTool?.Invoke(
                $"--build --root-owner-group \"{this.DebDirectory}\"",
                logger: (type, text) => Log.Debug(text));

            this.DebDirectory.DeleteDirectory();
        });

    public Target PrepareRpmPackage => t => t
        .Description("Prepares files for creating an RPM package containing the published project")
        .DependsOn(this.Publish)
        .Requires(() => this.TargetOS == TargetOS.Linux)
        .Executes(() =>
        {
            Log.Information("Preparing files for creating an RPM package containing the published project");

            RpmDirectory.CreateOrCleanDirectory();

            CopyFile(this.SourceRpmSpecFile, this.TargetRpmSpecFile, FileExistsPolicy.Overwrite);
            CopyFile(SourceLicenseFile, TargetRpmLicenseFile, FileExistsPolicy.Overwrite);
            CopyFileToDirectory(SourceLinuxIconFile, PublishOutputDirectory, FileExistsPolicy.Overwrite);

            ResolvePlaceholders(this.TargetRpmSpecFile, this.Platform.Rpm);
        });

    public Target CreateRpmPackage => t => t
        .Description("Creates an RPM package containing the published project")
        .DependsOn(this.PrepareRpmPackage)
        .Requires(() => OperatingSystem.IsLinux(), () => this.TargetOS == TargetOS.Linux)
        .Requires(() => this.RpmBuild)
        .Produces(this.RpmFile)
        .Executes(() =>
        {
            Assert.NotNull(this.RpmBuild, "rpmbuild is not available");

            Log.Information("Creating an RPM package containing the published project");

            this.RpmBuild?.Invoke(
                $"-bb --build-in-place --define \"_topdir {RpmDirectory}\" " +
                $"--target {this.Platform.Rpm} \"{this.TargetRpmSpecFile}\"",
                logger: (type, text) => Log.Debug(text));

            CopyFile(this.RpmOutputFile, this.RpmFile, FileExistsPolicy.Overwrite);

            RpmDirectory.DeleteDirectory();
            TargetRpmLicenseFile.DeleteFile();
            this.TargetRpmSpecFile.DeleteFile();
        });

    public Target LocalCleanUp => t => t
        .Description("Deletes the publish directory")
        .TriggeredBy(this.CreateArchive, this.CreateMacOSPackage, this.CreateDebianPackage, this.CreateRpmPackage)
        .OnlyWhenStatic(() => IsLocalBuild)
        .Unlisted()
        .Executes(() =>
        {
            Log.Information("Deleting the publish directory");
            PublishOutputDirectory.DeleteDirectory();
        });
}

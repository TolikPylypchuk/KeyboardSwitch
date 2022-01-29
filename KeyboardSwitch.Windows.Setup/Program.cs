using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

using WixSharp;

using Assembly = System.Reflection.Assembly;

using File = System.IO.File;
using WixFile = WixSharp.File;

namespace KeyboardSwitch.Windows.Setup
{
    public class Program
    {
        private const string ProductName = "Keyboard Switch";
        private const string ProductSettingsName = "Keyboard Switch Settings";

        private const string BuildDirectory = @"..\bin\publish";
        private static readonly string TargetDirectory = @"%ProgramFiles%\Keyboard Switch";

        private static readonly List<string> ExcludedFileExtensions = new List<string>
        {
            ".pdb",
            ".xml",
            ".windows.json",
            ".macos.json",
            ".linux.json",
            ".icns",
            ".png"
        };

        private const string InstallationDirectory = "[INSTALLDIR]";
        private const string StartMenuDirectory = "%ProgramMenu%";

        private readonly static string ExplorerPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

        private const string SettingsAppId = "KeyboardSwitchSettings_exe";
        private const string SettingsAppName = "KeyboardSwitchSettings.exe";

        private const string DeleteConfigKey = "9000";

        private const string LocalAppDataRegistryValue = "Local AppData";
        private const string ShellFoldersRegistryKeyFormat =
            @"HKEY_USERS\{0}\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";

        private const string SetupConfiguredFile = ".setup-configured";

        private static void Main()
        {
            var files = new Dir(
                TargetDirectory,
                new Files(
                    @$"{BuildDirectory}\*.*",
                    file => !file.EndsWith(SettingsAppName) && !ExcludedFileExtensions.Any(file.EndsWith)),
                new WixFile(new Id(SettingsAppId), @$"{BuildDirectory}\{SettingsAppName}"),
                new WixFile(Path.Combine(BuildDirectory, "appsettings.windows.json"))
                {
                    AttributesDefinition = "Name=appsettings.json"
                });

            var startMenuShortcut = new Dir(
                StartMenuDirectory,
                new ExeFileShortcut(
                    ProductSettingsName, Path.Combine(InstallationDirectory, SettingsAppName), arguments: String.Empty)
                {
                    WorkingDirectory = InstallationDirectory
                });

            var deleteConfigMessage = new Error(DeleteConfigKey, "Do you want to delete the app's configuration?");

            var launch = new LaunchApplicationFromExitDialog(SettingsAppId, "Launch Keyboard Switch Settings");

            var currentAssembly = Assembly.GetExecutingAssembly();

            var project = new ManagedProject(ProductName, files, startMenuShortcut, launch, deleteConfigMessage)
            {
                GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b"),
                InstallScope = InstallScope.perMachine,
                InstallerVersion = 500,
                InstallPrivileges = InstallPrivileges.elevated,
                Platform = Platform.x64,
                OutDir = Path.GetDirectoryName(currentAssembly.GetLocation()),
                LicenceFile = "License.rtf",
                BannerImage = "Banner.bmp",
                BackgroundImage = "Dialog.bmp",
                ValidateBackgroundImage = false,
                MajorUpgradeStrategy = MajorUpgradeStrategy.Default,
                UI = WUI.WixUI_InstallDir,
                Version = currentAssembly.GetName().Version
            };

            project.ControlPanelInfo.NoModify = true;
            project.ControlPanelInfo.NoRepair = true;
            project.ControlPanelInfo.ProductIcon = "icon.ico";
            project.ControlPanelInfo.InstallLocation = InstallationDirectory;

            project.ControlPanelInfo.Manufacturer = currentAssembly
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute))
                .OfType<AssemblyCompanyAttribute>()
                .First()
                .Company;

            project.MajorUpgradeStrategy.RemoveExistingProductAfter = Step.InstallInitialize;

            project.AfterInstall += AfterInstall;

            project.BuildMsi();
        }

        private static void AfterInstall(SetupEventArgs e)
        {
            if (e.IsUninstalling && !e.IsUpgradingInstalledVersion)
            {
                try
                {
                    using var record = new Record(1);
                    record[1] = DeleteConfigKey;

                    var messageType = InstallMessage.User |
                        (InstallMessage)MessageButtons.YesNo |
                        (InstallMessage)MessageIcon.Question;

                    var directories = GetConfigDirectories();

                    if (e.Session.Message(messageType, record) == MessageResult.Yes)
                    {
                        foreach (var directory in directories.Where(Directory.Exists))
                        {
                            Directory.Delete(directory, recursive: true);
                        }
                    } else
                    {
                        foreach (var file in directories
                            .Select(directory => Path.Combine(directory, SetupConfiguredFile))
                            .Where(File.Exists))
                        {
                            File.Delete(file);
                        }
                    }
                } catch (Exception exp)
                {
                    e.Session.Log($"An exception occured when trying to delete the configuration: {exp}");
                }
            }
        }

        private static List<string> GetConfigDirectories() =>
            Registry.Users.GetSubKeyNames()
                .Select(sid => String.Format(ShellFoldersRegistryKeyFormat, sid))
                .Select(key => Registry.GetValue(key, LocalAppDataRegistryValue, null))
                .OfType<string>()
                .Where(value => !String.IsNullOrEmpty(value))
                .Select(value => Path.Combine(value, nameof(KeyboardSwitch)))
                .ToList();
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Deployment.WindowsInstaller;

using WixSharp;

using Assembly = System.Reflection.Assembly;

using File = WixSharp.File;

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

        private const string SettingsAppName = "KeyboardSwitchSettings.exe";

        private const string DeleteConfigKey = "9000";

        private static readonly string DefaultDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(KeyboardSwitch));

        private static void Main()
        {
            var files = new Dir(
                TargetDirectory,
                new Files(@$"{BuildDirectory}\*.*", file => !ExcludedFileExtensions.Any(file.EndsWith)),
                new File(Path.Combine(BuildDirectory, "appsettings.windows.json"))
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

            var currentAssembly = Assembly.GetExecutingAssembly();

            var project = new ManagedProject(ProductName, files, startMenuShortcut, deleteConfigMessage)
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

            project.BeforeInstall += BeforeInstall;
            project.AfterInstall += AfterInstall;

            project.BuildMsi();
        }

        private static void BeforeInstall(SetupEventArgs e)
        {
            if (e.IsUninstalling)
            {
                try
                {
                    using var record = new Record(1);
                    record[1] = DeleteConfigKey;

                    var messageType = InstallMessage.User |
                        (InstallMessage)MessageButtons.YesNo |
                        (InstallMessage)MessageIcon.Question;

                    if (e.Session.Message(messageType, record) == MessageResult.Yes)
                    {
                        Directory.Delete(DefaultDataDirectory, recursive: true);
                    }
                } catch (Exception exp)
                {
                    e.Session.Log($"An exception occured when trying to ask about deleting the configuration: {exp}");
                }
            }
        }

        private static void AfterInstall(SetupEventArgs e)
        {
            if (!e.IsUninstalling)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = ExplorerPath,
                        Arguments = $"\"{Path.Combine(e.InstallDir, SettingsAppName)}\"",
                        WorkingDirectory = e.InstallDir
                    });
                } catch (Exception exp)
                {
                    e.Session.Log($"An exception occured when trying to start the Keyboard Switch Settings app: {exp}");
                }
            }
        }
    }
}

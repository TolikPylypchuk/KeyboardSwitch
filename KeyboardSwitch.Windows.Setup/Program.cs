using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using WixSharp;

using Assembly = System.Reflection.Assembly;

namespace KeyboardSwitch.Windows.Setup
{
    public class Program
    {
        private const string ProductName = "Keyboard Switch";
        private const string ProductSettingsName = "Keyboard Switch Settings";

        private const string BuildDirectory = @"..\bin\publish\*.*";
        private static readonly string TargetDirectory = @$"%ProgramFiles%\{nameof(KeyboardSwitch)}";
        private const string InstallationDirectory = "[INSTALLDIR]";
        private const string StartMenuDirectory = "%ProgramMenu%";

        private readonly static string ExplorerPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

        private const string SettingsAppName = "KeyboardSwitchSettings.exe";

        private static void Main()
        {
            var files = new Dir(TargetDirectory, new Files(BuildDirectory));

            var startMenuShortcut = new Dir(
                StartMenuDirectory,
                new ExeFileShortcut(
                    ProductSettingsName, Path.Combine(InstallationDirectory, SettingsAppName), arguments: String.Empty)
                {
                    WorkingDirectory = InstallationDirectory
                });

            var currentAssembly = Assembly.GetExecutingAssembly();

            var project = new ManagedProject(ProductName, files, startMenuShortcut)
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

using System;
using System.IO;

using WixSharp;
using WixSharp.Forms;

using Assembly = System.Reflection.Assembly;

namespace KeyboardSwitch.Windows.Setup
{
    public class Program
    {
        private const string BuildDirectory = @"..\bin\publish\*.*";
        private static readonly string TargetDirectory = @$"%ProgramFiles%\{nameof(KeyboardSwitch)}";

        private const string MsiName = "KeyboardSwitchSetup.msi";
        private static readonly string MsiPath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocation()), MsiName);

        private static void Main()
        {
            var project = new ManagedProject(nameof(KeyboardSwitch),
                             new Dir(TargetDirectory, new Files(BuildDirectory)))
            {
                GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b"),
                ManagedUI = ManagedUI.Default
            };

            project.ManagedUI.InstallDialogs
                .Add(Dialogs.Welcome)
                .Add(Dialogs.Licence)
                .Add(Dialogs.InstallDir)
                .Add(Dialogs.Progress)
                .Add(Dialogs.Exit);

            project.Load += MsiLoad;
            project.BeforeInstall += MsiBeforeInstall;
            project.AfterInstall += MsiAfterInstall;

            project.BuildMsi(MsiPath);
        }

        private static void MsiLoad(SetupEventArgs e)
        {
        }

        private static void MsiBeforeInstall(SetupEventArgs e)
        {
        }

        private static void MsiAfterInstall(SetupEventArgs e)
        {
        }
    }
}

using System;

using WixSharp;
using WixSharp.Forms;

namespace KeyboardSwitch.Windows.Setup
{
    public class Program
    {
        private static void Main()
        {
            var project = new ManagedProject(nameof(KeyboardSwitch),
                             new Dir(@$"%ProgramFiles%\{nameof(KeyboardSwitch)}",
                                 new File($"{nameof(Program)}.cs")))
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

            project.BuildMsi();
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

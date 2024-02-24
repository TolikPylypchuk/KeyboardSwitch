namespace KeyboardSwitch.Linux.Services;

internal sealed class GnomeLayoutService(X11Service x11, ILogger<GnomeLayoutService> logger)
    : XLayoutService(x11, logger)
{
    private const string SwitchLayout = "switch-layout@tolik.io";
    private const string Bash = "bash";
    private const string GnomeExtensions = "gnome-extensions";
    private const string GnomeShellExtensionTool = "gnome-shell-extension-tool";

    private bool isSwitchLayoutExtensionEnabled = false;

    private protected override void SetLayout(uint group)
    {
        if (!this.isSwitchLayoutExtensionEnabled)
        {
            this.EnableSwitchLayoutExtension();
        }

        Process.Start(
            "gdbus",
            "call --session --dest org.gnome.Shell --object-path /org/gnome/Shell/Extensions/SwitchLayout " +
            $"--method org.gnome.Shell.Extensions.SwitchLayout.Call {group}");
    }

    private void EnableSwitchLayoutExtension()
    {
        logger.LogDebug("Enabling the Switch Layout extension for GNOME");

        var command = Process.Start(Bash, $"-c \"command -v {GnomeExtensions}\"");
        command.WaitForExit();

        if (command.ExitCode == 0)
        {
            Process.Start(Bash, $"-c \"{GnomeExtensions} enable {SwitchLayout}\"");
        } else
        {
            Process.Start(Bash, $"-c \"{GnomeShellExtensionTool} -e {SwitchLayout}\"");
        }

        this.isSwitchLayoutExtensionEnabled = true;

        Thread.Sleep(50);
    }
}

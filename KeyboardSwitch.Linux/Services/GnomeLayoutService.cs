namespace KeyboardSwitch.Linux.Services;

using System.Diagnostics;

internal sealed class GnomeLayoutService : XLayoutService
{
    public GnomeLayoutService(ILogger<GnomeLayoutService> logger)
        : base(logger)
    { }

    private protected override void SetLayout(XDisplayHandle display, uint group) =>
        Process.Start(
            "gdbus",
            "call --session --dest org.gnome.Shell --object-path /org/gnome/Shell --method org.gnome.Shell.Eval " +
            $"\"imports.ui.status.keyboard.getInputSourceManager().inputSources[{group}].activate()\"");
}

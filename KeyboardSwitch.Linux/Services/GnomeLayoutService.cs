namespace KeyboardSwitch.Linux.Services;

internal sealed class GnomeLayoutService : XLayoutService
{
    public GnomeLayoutService(ILogger<GnomeLayoutService> logger)
        : base(logger)
    { }

    private protected override void SetLayout(XDisplayHandle display, uint group) =>
        Process.Start(
            "gdbus",
            "call --session --dest org.gnome.Shell --object-path /org/gnome/Shell/Extensions/SwitchLayout " +
            $"--method org.gnome.Shell.Extensions.SwitchLayout.Call {group}");
}

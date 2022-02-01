namespace KeyboardSwitch.Linux.Services;

internal sealed class GnomeSetupService : StartupSetupService
{
    private const string SwitchLayout = "switch-layout@tolik.io";
    private const string GnomeExtensions = "gnome-extensions";
    private const string GnomeShellExtensionTool = "gnome-shell-extension-tool";

    private readonly ILogger<GnomeSetupService> logger;

    public GnomeSetupService(
        IStartupService startupService,
        IOptions<GlobalSettings> globalSettings,
        ILogger<GnomeSetupService> logger)
        : base(startupService, globalSettings, logger) =>
        this.logger = logger;

    public override void DoInitialSetup()
    {
        base.DoInitialSetup();

        this.logger.LogInformation("Enabling the {Extension} extension for GNOME", SwitchLayout);

        var command = Process.Start("command", $"-v {GnomeExtensions}");
        command.WaitForExit();

        if (command.ExitCode == 0)
        {
            Process.Start(GnomeExtensions, $"enable {SwitchLayout}");
        } else
        {
            Process.Start(GnomeShellExtensionTool, $"-e {SwitchLayout}");
        }
    }
}

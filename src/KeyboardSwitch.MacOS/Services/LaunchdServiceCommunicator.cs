namespace KeyboardSwitch.MacOS.Services;

internal sealed class LaunchdServiceCommunicator(
    INamedPipeService namedPipeService,
    IOptions<GlobalSettings> globalSettings,
    IOptions<LaunchdSettings> launchdSettings,
    ILogger<LaunchdServiceCommunicator> logger)
    : DirectServiceCommunicator(namedPipeService, globalSettings, logger)
{
    private readonly string serviceName = launchdSettings.Value.ServiceName;

    public override void StartService()
    {
        try
        {
            Process.Start(LaunchCtl, $"start {this.serviceName}");
        } catch (Exception e)
        {
            this.LogCannotStartServiceApplication(e);
        }
    }
}

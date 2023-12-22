namespace KeyboardSwitch.MacOS.Services;

using System.Diagnostics;

internal sealed class LaunchdServiceCommunicator(
    INamedPipeService namedPipeService,
    IOptions<GlobalSettings> globalSettings,
    IOptions<LaunchdSettings> launchdSettings)
    : DirectServiceCommunicator(namedPipeService, globalSettings)
{
    private readonly string serviceName = launchdSettings.Value.ServiceName;

    public override void StartService() =>
        Process.Start(LaunchCtl, $"start {this.serviceName}");
}

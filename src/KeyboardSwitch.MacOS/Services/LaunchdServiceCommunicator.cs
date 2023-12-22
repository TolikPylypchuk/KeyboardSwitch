namespace KeyboardSwitch.MacOS.Services;

using System.Diagnostics;

internal sealed class LaunchdServiceCommunicator(
    IOptions<GlobalSettings> globalSettings,
    IOptions<LaunchdSettings> launchdSettings,
    INamedPipeService namedPipeServiceProvider)
    : DirectServiceCommunicator(globalSettings, namedPipeServiceProvider)
{
    private readonly string serviceName = launchdSettings.Value.ServiceName;

    public override void StartService() =>
        Process.Start(LaunchCtl, $"start {this.serviceName}");
}

namespace KeyboardSwitch.MacOS.Services;

using System.Diagnostics;

using KeyboardSwitch.Core.Services;

internal sealed class LaunchdServiceCommunicator(
    IOptions<GlobalSettings> globalSettings,
    IOptions<LaunchdSettings> launchdSettings,
    ServiceProvider<INamedPipeService> namedPipeServiceProvider)
    : DirectServiceCommunicator(globalSettings, namedPipeServiceProvider)
{
    private readonly string serviceName = launchdSettings.Value.ServiceName;

    public override void StartService() =>
        Process.Start(LaunchCtl, $"start {this.serviceName}");
}

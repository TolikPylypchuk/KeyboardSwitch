namespace KeyboardSwitch.MacOS.Services;

using System.Diagnostics;

using KeyboardSwitch.Core.Services;

internal sealed class LaunchdServiceCommunicator : DirectServiceCommunicator
{
    private readonly string serviceName;

    public LaunchdServiceCommunicator(
        IOptions<GlobalSettings> globalSettings,
        IOptions<LaunchdSettings> launchdSettings,
        ServiceProvider<INamedPipeService> namedPipeServiceProvider)
        : base(globalSettings, namedPipeServiceProvider) =>
        this.serviceName = launchdSettings.Value.ServiceName;

    public override void StartService() =>
        Process.Start(LaunchCtl, $"start {this.serviceName}");
}

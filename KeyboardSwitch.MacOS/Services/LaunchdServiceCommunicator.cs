namespace KeyboardSwitch.MacOS.Services;

using System.Diagnostics;

using KeyboardSwitch.Core.Services;

internal sealed class LaunchdServiceCommunicator : DirectServiceCommunicator
{
    public LaunchdServiceCommunicator(
        IOptions<GlobalSettings> globalSettings,
        ServiceProvider<INamedPipeService> namedPipeServiceProvider)
        : base(globalSettings, namedPipeServiceProvider)
    { }

    public override void StartService() =>
        Process.Start(LaunchCtl, $"start {this.GlobalSettings.ServicePath}");
}

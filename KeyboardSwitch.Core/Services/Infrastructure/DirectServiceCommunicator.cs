namespace KeyboardSwitch.Core.Services.Infrastructure;

using System.Diagnostics;

public sealed class DirectServiceCommunicator : IServiceCommunicator
{
    private readonly GlobalSettings globalSettings;
    private readonly INamedPipeService namedPipeService;

    public DirectServiceCommunicator(
        IOptions<GlobalSettings> globalSettings,
        ServiceProvider<INamedPipeService> namedPipeServiceProvider)
    {
        this.globalSettings = globalSettings.Value;
        this.namedPipeService = namedPipeServiceProvider(nameof(KeyboardSwitch));
    }

    public bool IsServiceRunning() =>
        Process.GetProcessesByName(nameof(KeyboardSwitch)).Length > 0;

    public void StartService() =>
        Process.Start(globalSettings.ServicePath);

    public void StopService(bool kill)
    {
        if (kill)
        {
            Process.GetProcessesByName(nameof(KeyboardSwitch)).ForEach(process => process.Kill());
        } else
        {
            this.namedPipeService.Write(ExternalCommand.Stop);
        }
    }

    public void ReloadService() =>
        this.namedPipeService.Write(ExternalCommand.ReloadSettings);
}

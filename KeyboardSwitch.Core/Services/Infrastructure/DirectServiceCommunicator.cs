namespace KeyboardSwitch.Core.Services.Infrastructure;

using System.Diagnostics;

public class DirectServiceCommunicator(
    IOptions<GlobalSettings> globalSettings,
    ServiceProvider<INamedPipeService> namedPipeServiceProvider)
    : IServiceCommunicator
{
    private readonly GlobalSettings globalSettings = globalSettings.Value;
    private readonly INamedPipeService namedPipeService = namedPipeServiceProvider(nameof(KeyboardSwitch));

    public virtual bool IsServiceRunning() =>
        Process.GetProcessesByName(nameof(KeyboardSwitch)).Length > 0;

    public virtual void StartService() =>
        Process.Start(this.globalSettings.ServicePath);

    public virtual void StopService(bool kill)
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

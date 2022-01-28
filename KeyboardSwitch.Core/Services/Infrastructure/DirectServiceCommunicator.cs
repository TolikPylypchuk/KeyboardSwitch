namespace KeyboardSwitch.Core.Services.Infrastructure;

using System.Diagnostics;

public class DirectServiceCommunicator : IServiceCommunicator
{
    protected readonly GlobalSettings GlobalSettings;
    private readonly INamedPipeService namedPipeService;

    public DirectServiceCommunicator(
        IOptions<GlobalSettings> globalSettings,
        ServiceProvider<INamedPipeService> namedPipeServiceProvider)
    {
        this.GlobalSettings = globalSettings.Value;
        this.namedPipeService = namedPipeServiceProvider(nameof(KeyboardSwitch));
    }

    public virtual bool IsServiceRunning() =>
        Process.GetProcessesByName(nameof(KeyboardSwitch)).Length > 0;

    public virtual void StartService() =>
        Process.Start(this.GlobalSettings.ServicePath);

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

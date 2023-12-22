namespace KeyboardSwitch.Core.Services.Infrastructure;

using System.Diagnostics;

public class DirectServiceCommunicator(
    IOptions<GlobalSettings> globalSettings,
    INamedPipeService namedPipeService)
    : IServiceCommunicator
{
    private const string KeyboardSwitch = nameof(KeyboardSwitch);

    private readonly GlobalSettings globalSettings = globalSettings.Value;
    private readonly INamedPipeService namedPipeService = namedPipeService;

    public virtual bool IsServiceRunning() =>
        Process.GetProcessesByName(KeyboardSwitch).Length > 0;

    public virtual void StartService() =>
        Process.Start(this.globalSettings.ServicePath);

    public virtual void StopService(bool kill)
    {
        if (kill)
        {
            Process.GetProcessesByName(KeyboardSwitch).ForEach(process => process.Kill());
        } else
        {
            this.namedPipeService.Write(KeyboardSwitch, ExternalCommand.Stop);
        }
    }

    public void ReloadService() =>
        this.namedPipeService.Write(KeyboardSwitch, ExternalCommand.ReloadSettings);
}

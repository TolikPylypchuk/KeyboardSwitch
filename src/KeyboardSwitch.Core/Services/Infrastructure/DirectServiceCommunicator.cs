namespace KeyboardSwitch.Core.Services.Infrastructure;

using System.Diagnostics;

public class DirectServiceCommunicator(
    INamedPipeService namedPipeService,
    IOptions<GlobalSettings> globalSettings)
    : IServiceCommunicator
{
    public virtual bool IsServiceRunning() =>
        Process.GetProcessesByName(nameof(KeyboardSwitch)).Length > 0;

    public virtual void StartService() =>
        Process.Start(globalSettings.Value.ServicePath);

    public virtual void StopService(bool kill)
    {
        if (kill)
        {
            Process.GetProcessesByName(nameof(KeyboardSwitch)).ForEach(process => process.Kill());
        } else
        {
            namedPipeService.Write(nameof(KeyboardSwitch), ExternalCommand.Stop);
        }
    }

    public void ReloadService() =>
        namedPipeService.Write(nameof(KeyboardSwitch), ExternalCommand.ReloadSettings);
}

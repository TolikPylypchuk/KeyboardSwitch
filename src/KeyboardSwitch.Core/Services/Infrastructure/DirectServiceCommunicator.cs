using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace KeyboardSwitch.Core.Services.Infrastructure;

[ExcludeFromCodeCoverage]
public partial class DirectServiceCommunicator(
    INamedPipeService namedPipeService,
    IOptions<GlobalSettings> globalSettings,
    ILogger<DirectServiceCommunicator> logger)
    : IServiceCommunicator
{
    public virtual bool IsServiceRunning() =>
        Process.GetProcessesByName(nameof(KeyboardSwitch)).Length > 0;

    public virtual void StartService()
    {
        try
        {
            Process.Start(globalSettings.Value.ServicePath);
        } catch (Exception e)
        {
            this.LogCannotStartServiceApplication(e);
        }
    }

    public virtual void StopService(bool kill)
    {
        try
        {
            if (kill)
            {
                Process.GetProcessesByName(nameof(KeyboardSwitch)).ForEach(process => process.Kill());
            } else
            {
                namedPipeService.Write(nameof(KeyboardSwitch), ExternalCommand.Stop);
            }
        } catch (Exception e)
        {
            this.LogCannotStopServiceApplication(e);
        }
    }

    public void ReloadService() =>
        namedPipeService.Write(nameof(KeyboardSwitch), ExternalCommand.ReloadSettings);

    [LoggerMessage("Cannot start the service application.", Level = LogLevel.Error)]
    protected partial void LogCannotStartServiceApplication(Exception exception);

    [LoggerMessage("Cannot stop the service application.", Level = LogLevel.Error)]
    protected partial void LogCannotStopServiceApplication(Exception exception);
}

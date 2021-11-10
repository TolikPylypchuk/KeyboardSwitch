namespace KeyboardSwitch.Core.Services.Infrastructure;

public interface IServiceCommunicator
{
    bool IsServiceRunning();
    Task StartServiceAsync();
    void StopService(bool kill);
    void ReloadService();
}

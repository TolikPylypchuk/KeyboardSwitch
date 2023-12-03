namespace KeyboardSwitch.Core.Services.Infrastructure;

public interface IServiceCommunicator
{
    bool IsServiceRunning();
    void StartService();
    void StopService(bool kill);
    void ReloadService();
}

using System.Threading.Tasks;

namespace KeyboardSwitch.Core.Services
{
    public interface IServiceCommunicator
    {
        bool IsServiceRunning();
        Task StartServiceAsync();
        void StopService(bool kill);
        void ReloadService();
    }
}

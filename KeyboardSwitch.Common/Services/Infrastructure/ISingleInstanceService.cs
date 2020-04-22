using System.Threading;

namespace KeyboardSwitch.Common.Services.Infrastructure
{
    public interface ISingleInstanceService
    {
        Mutex TryAcquireMutex();
    }
}

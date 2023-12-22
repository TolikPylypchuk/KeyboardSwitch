namespace KeyboardSwitch.Core.Services.Infrastructure;

public interface ISingleInstanceService
{
    Mutex TryAcquireMutex(string name);
}

namespace KeyboardSwitch.Linux.Session;

internal interface ILockStateClient
{
    Task<bool> GetLockState();

    ValueTask<IDisposable> WatchLockState(Action<Exception?, bool> handler);
}

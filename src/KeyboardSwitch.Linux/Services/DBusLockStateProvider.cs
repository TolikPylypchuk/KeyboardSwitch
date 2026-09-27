using System.Reactive.Linq;

namespace KeyboardSwitch.Linux.Services;

internal sealed partial class DBusLockStateProvider(
    IEnumerable<ILockStateClient> clients,
    ILogger<DBusLockStateProvider> logger)
    : ILockStateProvider
{
    public IObservable<bool> IsScreenLocked =>
        clients
            .Select(this.WatchLockState)
            .CombineLatest()
            .Select(states => states.Any(isLocked => isLocked))
            .DistinctUntilChanged();

    private IObservable<bool> WatchLockState(ILockStateClient client) =>
        Observable.Create<bool>(async observer =>
            {
                var gate = new Lock();
                bool isChanged = false;

                var watcher = await client.WatchLockState((e, isLocked) =>
                {
                    lock (gate)
                    {
                        if (e is null)
                        {
                            isChanged = true;
                            observer.OnNext(isLocked);
                        } else
                        {
                            observer.OnError(e);
                        }
                    }
                });

                try
                {
                    bool isLocked = await client.GetLockState();

                    lock (gate)
                    {
                        if (!isChanged)
                        {
                            observer.OnNext(isLocked);
                        }
                    }

                    return watcher;
                } catch
                {
                    watcher.Dispose();
                    throw;
                }
            })
            .Catch((Exception e) =>
            {
                this.LogCannotWatchLockState(e, client.GetType().Name);
                return Observable.Return(false);
            });

    [LoggerMessage(LogLevel.Warning, "Cannot watch the lock state using {Client}")]
    private partial void LogCannotWatchLockState(Exception e, string client);
}

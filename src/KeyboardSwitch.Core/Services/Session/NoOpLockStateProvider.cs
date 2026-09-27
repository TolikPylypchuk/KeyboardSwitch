namespace KeyboardSwitch.Core.Services.Session;

public sealed class NoOpLockStateProvider : ILockStateProvider
{
    public IObservable<bool> IsScreenLocked =>
        Observable.Return(false);
}

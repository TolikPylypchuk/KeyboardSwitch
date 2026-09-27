namespace KeyboardSwitch.Core.Services.Session;

public interface ILockStateProvider
{
    IObservable<bool> IsScreenLocked { get; }
}

namespace KeyboardSwitch.Core.Services.Hook;

public interface IKeyboardHookService : IDisposable
{
    IObservable<EventMask> HotKeyPressed { get; }

    void Register(IEnumerable<EventMask> modifiers, int pressedCount, int waitMilliseconds);

    void UnregisterAll();

    Task StartHook(CancellationToken token);
}

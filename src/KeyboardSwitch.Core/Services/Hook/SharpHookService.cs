using System.Reactive.Disposables;

using SharpHook.Providers;

namespace KeyboardSwitch.Core.Services.Hook;

internal sealed class SharpHookService : DisposableService, IKeyboardHookService
{
    internal const uint AxPollFrequencySeconds = 5;

    private static readonly TimeSpan KeyPressWaitThreshold = TimeSpan.FromSeconds(3);

    private DateTimeOffset lastKeyPress = DateTimeOffset.MinValue;

    private readonly IReactiveGlobalHook hook;
    private readonly IScheduler scheduler;
    private readonly ILogger<SharpHookService> logger;

    private readonly HashSet<EventMask> hotKeys = [];

    private readonly Subject<EventMask> rawHotKeyPressedSubject = new();
    private readonly Subject<EventMask> hotKeyPressedSubject = new();
    private readonly CompositeDisposable hotKeyPressedSubscriptions = [];

    private readonly HashSet<KeyCode> pressedKeys = [];
    private readonly HashSet<KeyCode> releasedKeys = [];

    private readonly IDisposable hookSubscription;

    public SharpHookService(
        IReactiveGlobalHook hook,
        IScheduler scheduler,
        IAccessibilityProvider accessibilityProvider,
        ILogger<SharpHookService> logger)
    {
        this.hook = hook;
        this.scheduler = scheduler;
        this.logger = logger;

        accessibilityProvider.AxPollFrequency = AxPollFrequencySeconds;

        this.hook.HookEnabled.Subscribe(e => this.logger.LogInformation("Created a global keyboard hook"));

        this.hookSubscription = this.hook.KeyPressed
            .Merge(this.hook.KeyReleased)
            .Delay(TimeSpan.FromMilliseconds(16), scheduler)
            .Subscribe(args =>
            {
                if (args.RawEvent.Type == EventType.KeyPressed)
                {
                    this.HandleKeyDown(args.Data.KeyCode);
                } else
                {
                    this.HandleKeyUp(args.Data.KeyCode);
                }
            });
    }

    public IObservable<EventMask> HotKeyPressed =>
        this.hotKeyPressedSubject.AsObservable();

    public void Register(IEnumerable<EventMask> modifiers, int pressedCount, int waitMilliseconds)
    {
        this.ThrowIfDisposed();

        var hotKey = modifiers.ToArray().Merge();

        this.logger.LogDebug("Registering a hot key: {HotKey}", hotKey);

        this.hotKeys.Add(hotKey);

        var subscription = pressedCount == 1
            ? this.SubscribeToKeyPressesSimple(hotKey)
            : this.SubscribeToKeyPresses(hotKey, pressedCount, TimeSpan.FromMilliseconds(waitMilliseconds));

        this.hotKeyPressedSubscriptions.Add(subscription);

        this.logger.LogDebug("Registered a hot key: {HotKey}", hotKey);
    }

    public void UnregisterAll()
    {
        this.ThrowIfDisposed();

        this.logger.LogDebug("Unregistering all hot keys");

        this.hotKeys.Clear();
        this.hotKeyPressedSubscriptions.Clear();

        this.logger.LogDebug("Unregistered all hot keys");
    }

    public async Task StartHook(CancellationToken token)
    {
        token.Register(this.hook.Stop);
        await this.hook.RunAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.logger.LogDebug("Destroying the global hook");

            this.rawHotKeyPressedSubject.OnCompleted();
            this.hotKeyPressedSubject.OnCompleted();

            this.hookSubscription.Dispose();
            this.rawHotKeyPressedSubject.Dispose();
            this.hotKeyPressedSubject.Dispose();
            this.hotKeyPressedSubscriptions.Dispose();
        }
    }

    private IDisposable SubscribeToKeyPressesSimple(EventMask modifier) =>
        this.rawHotKeyPressedSubject
            .Where(key => key.IsSubsetKeyOf(modifier))
            .Subscribe(this.hotKeyPressedSubject);

    private IDisposable SubscribeToKeyPresses(EventMask modifier, int pressedCount, TimeSpan waitTime) =>
        this.rawHotKeyPressedSubject
            .Where(key => key.IsSubsetKeyOf(modifier))
            .Buffer(this.rawHotKeyPressedSubject
                .Scan(
                    DateTimeOffset.MinValue,
                    (lastKeyPressTime, _) => this.scheduler.Now - lastKeyPressTime > waitTime
                        ? this.scheduler.Now
                        : lastKeyPressTime)
                .Delay(waitTime, scheduler))
            .Where(modifiers => modifiers.Count == pressedCount && modifiers.All(m => m == modifiers[0]))
            .Select(modifiers => modifiers[0])
            .Subscribe(this.hotKeyPressedSubject);

    private void HandleKeyDown(KeyCode keyCode)
    {
        if (this.pressedKeys.Contains(keyCode))
        {
            return;
        }

        this.logger.LogDebug("Received key down: {KeyCode}", keyCode);

        if (this.scheduler.Now - this.lastKeyPress > KeyPressWaitThreshold)
        {
            this.pressedKeys.Clear();
        }

        this.releasedKeys.Clear();
        this.pressedKeys.Add(keyCode);
        this.lastKeyPress = this.scheduler.Now;
    }

    private void HandleKeyUp(KeyCode keyCode)
    {
        if (!this.pressedKeys.Contains(keyCode))
        {
            return;
        }

        this.logger.LogDebug("Received key up: {KeyCode}", keyCode);

        this.pressedKeys.Remove(keyCode);
        this.releasedKeys.Add(keyCode);

        if (this.scheduler.Now - this.lastKeyPress > KeyPressWaitThreshold)
        {
            this.releasedKeys.Clear();
        }

        if (this.pressedKeys.Count != 0 || this.releasedKeys.Any(key => key.ToEventMask() is null))
        {
            return;
        }

        var modifiers = this.releasedKeys
            .Select(key => key.ToEventMask()!.Value)
            .ToArray()
            .Merge();

        if (this.hotKeys.Any(hotKey => modifiers.IsSubsetKeyOf(hotKey)))
        {
            this.logger.LogDebug("Hot key activated: {HotKey}", modifiers);
            this.rawHotKeyPressedSubject.OnNext(modifiers);
        }
    }
}

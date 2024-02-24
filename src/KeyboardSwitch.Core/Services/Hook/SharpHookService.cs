namespace KeyboardSwitch.Core.Services.Hook;

using System.Reactive.Disposables;

internal sealed class SharpHookService : Core.Disposable, IKeyboardHookService
{
    private DateTimeOffset lastKeyPress = DateTimeOffset.MinValue;
    private readonly TimeSpan keyPressWaitThresshold = TimeSpan.FromSeconds(3);

    private readonly IReactiveGlobalHook hook;
    private readonly IScheduler scheduler;
    private readonly ILogger<SharpHookService> logger;

    private readonly HashSet<ModifierMask> hotKeys = [];

    private readonly Subject<ModifierMask> rawHotKeyPressedSubject = new();
    private readonly Subject<ModifierMask> hotKeyPressedSubject = new();
    private readonly CompositeDisposable hotKeyPressedSubscriptions = [];

    private readonly HashSet<KeyCode> pressedKeys = [];
    private readonly HashSet<KeyCode> releasedKeys = [];

    private readonly IDisposable hookSubscription;

    public SharpHookService(
        IReactiveGlobalHook hook,
        IScheduler scheduler,
        ILogger<SharpHookService> logger)
    {
        this.hook = hook;
        this.scheduler = scheduler;
        this.logger = logger;

        this.hookSubscription = this.hook.KeyPressed
            .Merge(this.hook.KeyReleased)
            .Delay(TimeSpan.FromMilliseconds(16))
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

    ~SharpHookService() =>
        this.Dispose(false);

    public IObservable<ModifierMask> HotKeyPressed =>
        this.hotKeyPressedSubject.AsObservable();

    public void Register(IEnumerable<ModifierMask> modifiers, int pressedCount, int waitMilliseconds)
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
        this.logger.LogDebug("Unregistering all hot keys");

        this.ThrowIfDisposed();

        this.hotKeys.Clear();
        this.hotKeyPressedSubscriptions.Clear();

        this.logger.LogDebug("Unregistered all hot keys");
    }

    public async Task StartHook(CancellationToken token)
    {
        this.hook.HookEnabled.Subscribe(e => this.logger.LogInformation("Created a global keyboard hook"));
        token.Register(this.hook.Dispose);
        await this.hook.RunAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.logger.LogDebug("Destroying the global hook");

            this.hookSubscription.Dispose();
            this.rawHotKeyPressedSubject.Dispose();
            this.hotKeyPressedSubject.Dispose();
            this.hotKeyPressedSubscriptions.Dispose();
        }
    }

    private IDisposable SubscribeToKeyPressesSimple(ModifierMask modifier) =>
        this.rawHotKeyPressedSubject
            .Where(key => key.IsSubsetKeyOf(modifier))
            .Subscribe(this.hotKeyPressedSubject);

    private IDisposable SubscribeToKeyPresses(ModifierMask modifier, int pressedCount, TimeSpan waitTime) =>
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
        if (this.scheduler.Now - this.lastKeyPress > this.keyPressWaitThresshold)
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

        this.pressedKeys.Remove(keyCode);
        this.releasedKeys.Add(keyCode);

        if (this.pressedKeys.Count != 0)
        {
            return;
        }

        var modifiers = this.releasedKeys
            .Select(key => key.ToModifierMask())
            .ToList();

        this.releasedKeys.Clear();

        if (!modifiers.Any(key => key is null) && modifiers.Count > 1)
        {
            var hotKey = modifiers.Select(key => key!.Value).ToArray().Merge();
            this.logger.LogDebug("Hot key activated: {HotKey}", hotKey);
            this.rawHotKeyPressedSubject.OnNext(hotKey);
        }
    }
}

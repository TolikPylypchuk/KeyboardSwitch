namespace KeyboardSwitch.Core.Services.Hook;

internal sealed class SharpHookService : Disposable, IKeyboardHookService
{
    private DateTimeOffset lastKeyPress = DateTimeOffset.MinValue;
    private readonly TimeSpan keyPressWaitThresshold = TimeSpan.FromSeconds(3);

    private readonly IReactiveGlobalHook hook;
    private readonly IScheduler scheduler;
    private readonly ILogger<SharpHookService> logger;

    private readonly HashSet<ModifierMask> hotKeys = new();

    private readonly Subject<ModifierMask> rawHotKeyPressedSubject = new();
    private readonly Subject<ModifierMask> hotKeyPressedSubject = new();
    private readonly Dictionary<ModifierMask, IDisposable> hotKeyPressedSubscriptions = new();

    private readonly HashSet<KeyCode> pressedKeys = new();
    private readonly HashSet<KeyCode> releasedKeys = new();

    public SharpHookService(
        IReactiveGlobalHook hook,
        IScheduler scheduler,
        ILogger<SharpHookService> logger)
    {
        this.hook = hook;
        this.scheduler = scheduler;
        this.logger = logger;

        this.hook.KeyPressed
            .Select(hookEvent => hookEvent.Args.Data.KeyCode)
            .Subscribe(this.HandleKeyDown);

        this.hook.KeyReleased
            .Select(hookEvent => hookEvent.Args.Data.KeyCode)
            .Subscribe(this.HandleKeyUp);
    }

    ~SharpHookService() =>
        this.Dispose(false);

    public IObservable<ModifierMask> HotKeyPressed =>
        this.hotKeyPressedSubject.AsObservable();

    public void Register(IEnumerable<ModifierMask> modifiers, int pressedCount, int waitMilliseconds)
    {
        this.ThrowIfDisposed();

        var hotKey = modifiers.Flatten();

        this.logger.LogDebug("Registering a hot key: {HotKey}", hotKey);

        this.hotKeys.Add(hotKey);

        var subscription = pressedCount == 1
            ? this.SubscribeToKeyPressesSimple(hotKey)
            : this.SubscribeToKeyPresses(hotKey, pressedCount, TimeSpan.FromMilliseconds(waitMilliseconds));

        this.hotKeyPressedSubscriptions.Add(hotKey, subscription);

        this.logger.LogDebug("Registered a hot key: {HotKey}", hotKey);
    }

    public void Unregister(IEnumerable<ModifierMask> modifiers)
    {
        this.ThrowIfDisposed();

        var hotKey = modifiers.Flatten();

        this.logger.LogDebug("Unregistering a hot key: {HotKey}", hotKey);

        if (!this.hotKeys.Contains(hotKey))
        {
            this.logger.LogWarning("Hot key {HotKey} not found", hotKey);
            return;
        }

        this.hotKeys.Remove(hotKey);

        this.hotKeyPressedSubscriptions[hotKey].Dispose();
        this.hotKeyPressedSubscriptions.Remove(hotKey);

        this.logger.LogDebug("Unregistered a hot key: {HotKey}", hotKey);
    }

    public void UnregisterAll()
    {
        this.logger.LogDebug("Unregistering all hot keys");

        this.ThrowIfDisposed();

        this.hotKeys.Clear();

        this.hotKeyPressedSubscriptions.Values.ForEach(subscription => subscription.Dispose());
        this.hotKeyPressedSubscriptions.Clear();

        this.logger.LogDebug("Unregistered all hot keys");
    }

    public async Task StartHook(CancellationToken token)
    {
        this.logger.LogInformation("Creating a global hook");
        token.Register(this.hook.Dispose);
        await this.hook.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.logger.LogDebug("Destroying a global hook");

            this.hook.Dispose();
            this.rawHotKeyPressedSubject.Dispose();
            this.hotKeyPressedSubject.Dispose();

            foreach (var subscription in this.hotKeyPressedSubscriptions.Values)
            {
                subscription.Dispose();
            }

            this.hotKeyPressedSubscriptions.Clear();
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
            var hotKey = modifiers.Select(key => key!.Value).Flatten();
            this.logger.LogDebug("Hot key activated: {HotKey}", hotKey);
            this.rawHotKeyPressedSubject.OnNext(hotKey);
        }
    }
}

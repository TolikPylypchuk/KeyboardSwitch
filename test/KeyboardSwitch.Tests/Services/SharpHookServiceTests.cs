using System.Reactive.Subjects;

using SharpHook.Providers;
using SharpHook.Reactive;
using SharpHook.Testing;

namespace KeyboardSwitch.Tests.Services;

[Properties(Arbitrary = [typeof(SharpHookServiceTests)])]
public sealed class SharpHookServiceTests(ITestOutputHelper output)
{
    private static readonly TimeSpan SmallDelay = TimeSpan.FromMilliseconds(32);

    private readonly ILogger<SharpHookService> logger = XUnitLogger.Create<SharpHookService>(output);

    public static Arbitrary<SingleModifier> ArbitrarySingleModifier =>
        new ArbitrarySingleModifier();

    public static Arbitrary<List<SingleModifier>> ArbitraryMultiModifier =>
        new ArbitraryModifiers();

    public static Arbitrary<NonModifierKey> ArbitraryNonModifierKey =>
        new ArbitraryNonModifierKey();

    public static Arbitrary<PressCount> ArbitraryPressCount =>
        new ArbitraryPressCount();

    public static Arbitrary<WaitTime> ArbitraryWaitTime =>
        new ArbitraryWaitTime();

    [Property(DisplayName = "Modifiers pressed once should work as a hot key")]
    public void ModifiersOnce(List<SingleModifier> modifiers, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        var expectedEventMask = modifiers.Select(modifier => modifier.Mask).ToArray();

        // Act

        service.Register(expectedEventMask, 1, waitTime.Value);

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        this.SimulateKeyEvents(hook, scheduler, modifiers.Select(modifier => modifier.KeyCode));

        // Assert

        Assert.Equal(1, observer.Messages.Count);
        Assert.Equal(expectedEventMask.Merge(), observer.Messages[0].Value.Value);
    }

    [Property(DisplayName = "Modifiers pressed multiple times should work as a hot key")]
    public void ModifiersMultiple(List<SingleModifier> modifiers, PressCount pressCount, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        var expectedEventMask = modifiers.Select(modifier => modifier.Mask).ToArray();

        // Act

        service.Register(expectedEventMask, pressCount.Value, waitTime.Value);

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        for (int i = 0; i < pressCount.Value; i++)
        {
            this.SimulateKeyEvents(hook, scheduler, modifiers.Select(modifier => modifier.KeyCode));
        }

        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(waitTime.Value).Ticks + SmallDelay.Ticks);

        // Assert

        Assert.Equal(1, observer.Messages.Count);
        Assert.Equal(expectedEventMask.Merge(), observer.Messages[0].Value.Value);
    }

    [Property(DisplayName = "Modifiers pressed multiple times should not work as a hot key if the break is too long")]
    public void ModifiersMultipleTooLong(List<SingleModifier> modifiers, PressCount pressCount, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        // Act

        service.Register(modifiers.Select(modifier => modifier.Mask), pressCount.Value, waitTime.Value);

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        long waitTimeTicks = TimeSpan.FromMilliseconds(waitTime.Value).Ticks;

        for (int i = 0; i < pressCount.Value; i++)
        {
            this.SimulateKeyEvents(hook, scheduler, modifiers.Select(modifier => modifier.KeyCode));
            scheduler.AdvanceBy(waitTimeTicks);
        }

        scheduler.AdvanceBy(waitTimeTicks + SmallDelay.Ticks);

        // Assert

        Assert.Equal(0, observer.Messages.Count);
    }

    [Property(DisplayName = "Other keys being pressed should disable the hot key")]
    public void OtherKeys(List<SingleModifier> modifiers, NonModifierKey key, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        // Act

        service.Register(modifiers.Select(modifier => modifier.Mask), 1, waitTime.Value);

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        this.SimulateKeyEvents(hook, scheduler, modifiers.Select(modifier => modifier.KeyCode).Append(key.Value));

        // Assert

        Assert.Equal(0, observer.Messages.Count);
    }

    [Property(DisplayName = "Key up events should be ignored without corresponding key down events")]
    public void OnlyKeyUp(List<SingleModifier> modifiers, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        // Act

        service.Register(modifiers.Select(modifier => modifier.Mask), 1, waitTime.Value);

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        foreach (var keyCode in modifiers.Select(modifier => modifier.KeyCode))
        {
            hook.SimulateKeyRelease(keyCode);
            scheduler.AdvanceBy(SmallDelay.Ticks);
        }

        // Assert

        Assert.Equal(0, observer.Messages.Count);
    }

    [Property(DisplayName = "Modifiers pressed down for too long should disable the hot key")]
    public void KeyDownTooLong(List<SingleModifier> modifiers, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        var expectedEventMask = modifiers.Select(modifier => modifier.Mask);

        // Act

        service.Register(expectedEventMask, 1, waitTime.Value);

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        this.SimulateKeyEvents(
            hook, scheduler, modifiers.Select(modifier => modifier.KeyCode), TimeSpan.FromSeconds(4));

        // Assert

        Assert.Equal(0, observer.Messages.Count);
    }

    [Property(DisplayName = "Repeated key down events should be ignored")]
    public void KeyDownRepeated(List<SingleModifier> modifiers, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        var expectedEventMask = modifiers.Select(modifier => modifier.Mask).ToArray();

        // Act

        service.Register(expectedEventMask, 1, waitTime.Value);

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        hook.SimulateKeyPress(modifiers[0].KeyCode);
        scheduler.AdvanceBy(SmallDelay.Ticks);

        this.SimulateKeyEvents(hook, scheduler, modifiers.Select(modifier => modifier.KeyCode));

        // Assert

        Assert.Equal(1, observer.Messages.Count);
        Assert.Equal(expectedEventMask.Merge(), observer.Messages[0].Value.Value);
    }

    [Property(DisplayName = "Modifiers should not work as a hot key after unregistering them")]
    public void UnregisterModifiers(List<SingleModifier> modifiers, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        // Act

        service.Register(modifiers.Select(modifier => modifier.Mask), 1, waitTime.Value);
        service.UnregisterAll();

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        this.SimulateKeyEvents(hook, scheduler, modifiers.Select(modifier => modifier.KeyCode));

        // Assert

        Assert.Equal(0, observer.Messages.Count);
    }

    [Fact(DisplayName = "Dispose should complete the hot key observable")]
    public void DisposeComplete()
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        // Act

        service.Dispose();

        // Assert

        Assert.Equal(1, observer.Messages.Count);
        Assert.Equal(NotificationKind.OnCompleted, observer.Messages[0].Value.Kind);
    }

    [Fact(DisplayName = "Cancelling the global hook should dispose of it")]
    public void Cancel()
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        // Act

        var tokenSource = new CancellationTokenSource();
        _ = service.StartHook(tokenSource.Token);
        this.WaitToStart(hook);

        tokenSource.Cancel();

        // Assert

        Assert.False(hook.IsRunning);
    }

    [Fact(DisplayName = "Locking the screen should stop the global hook")]
    public async Task LockScreen()
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();
        var lockStateProvider = new TestLockStateProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler), lockStateProvider, scheduler, provider, this.logger);

        // Act

        var hookTask = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        lockStateProvider.IsScreenLockedSubject.OnNext(true);
        await this.WaitToStop(hook);

        // Assert

        Assert.False(hook.IsRunning);
        Assert.False(hookTask.IsCompleted);
    }

    [Property(DisplayName = "Unlocking the screen should restart the global hook")]
    public void UnlockScreen(List<SingleModifier> modifiers, WaitTime waitTime)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();
        var lockStateProvider = new TestLockStateProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler), lockStateProvider, scheduler, provider, this.logger);

        var observer = scheduler.CreateObserver<EventMask>();
        service.HotKeyPressed.Subscribe(observer);

        var expectedEventMask = modifiers.Select(modifier => modifier.Mask).ToArray();

        // Act

        service.Register(expectedEventMask, 1, waitTime.Value);

        _ = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        lockStateProvider.IsScreenLockedSubject.OnNext(true);
        this.WaitToStop(hook).Wait();

        lockStateProvider.IsScreenLockedSubject.OnNext(false);
        this.WaitToStart(hook);

        this.SimulateKeyEvents(hook, scheduler, modifiers.Select(modifier => modifier.KeyCode));

        // Assert

        Assert.Equal(1, observer.Messages.Count);
        Assert.Equal(expectedEventMask.Merge(), observer.Messages[0].Value.Value);
    }

    [Fact(DisplayName = "The global hook should be stopped if it starts while the screen is locked")]
    public async Task StartWhileLocked()
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();
        var lockStateProvider = new TestLockStateProvider();

        lockStateProvider.IsScreenLockedSubject.OnNext(true);

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler), lockStateProvider, scheduler, provider, this.logger);

        // Act

        var hookTask = service.StartHook(CancellationToken.None);
        this.WaitToStart(hook);

        scheduler.AdvanceBy(SmallDelay.Ticks);
        await this.WaitToStop(hook);

        // Assert

        Assert.False(hook.IsRunning);
        Assert.False(hookTask.IsCompleted);
    }

    [Fact(DisplayName = "Cancelling the global hook while the screen is locked should complete it")]
    public async Task CancelWhileLocked()
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();
        var lockStateProvider = new TestLockStateProvider();

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler), lockStateProvider, scheduler, provider, this.logger);

        var tokenSource = new CancellationTokenSource();

        // Act

        var hookTask = service.StartHook(tokenSource.Token);
        this.WaitToStart(hook);

        lockStateProvider.IsScreenLockedSubject.OnNext(true);
        await this.WaitToStop(hook);

        tokenSource.Cancel();

        // Assert

        await hookTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.False(hook.IsRunning);
    }

    [Fact(DisplayName = "SharpHookService should set the poll frequency for the Accessibility API")]
    public void AccessibilityProvider()
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();
        var provider = new TestProvider();

        // Act

        using var service = new SharpHookService(
            new ReactiveGlobalHookAdapter(hook, scheduler),
            new NoOpLockStateProvider(),
            scheduler,
            provider,
            this.logger);

        // Assert

        Assert.Equal(SharpHookService.AxPollFrequencySeconds, ((IAccessibilityProvider)provider).AxPollFrequency);
    }

    private void SimulateKeyEvents(
        TestGlobalHook hook,
        TestScheduler scheduler,
        IEnumerable<KeyCode> keyCodes,
        TimeSpan? delay = null)
    {
        var actualDelay = delay ?? SmallDelay;

        foreach (var keyCode in keyCodes)
        {
            hook.SimulateKeyPress(keyCode);
            scheduler.AdvanceBy(actualDelay.Ticks);
        }

        foreach (var keyCode in keyCodes.Reverse())
        {
            hook.SimulateKeyRelease(keyCode);
            scheduler.AdvanceBy(actualDelay.Ticks);
        }
    }

    private void WaitToStart(TestGlobalHook hook)
    {
        while (!hook.IsRunning)
        {
            Thread.Sleep(SmallDelay);
        }
    }

    private async Task WaitToStop(TestGlobalHook hook)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        while (hook.IsRunning)
        {
            await Task.Delay(SmallDelay, tokenSource.Token);
        }
    }

    private sealed class TestLockStateProvider : ILockStateProvider
    {
        public BehaviorSubject<bool> IsScreenLockedSubject { get; } = new(false);

        public IObservable<bool> IsScreenLocked =>
            this.IsScreenLockedSubject;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Hook;
using KeyboardSwitch.Common.Keyboard;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Services
{
    internal sealed class UioHookService : Disposable, IKeyboardHookService
    {
        private readonly TaskQueue taskQueue = new();
        private readonly object keyLock = new();

        private readonly IScheduler scheduler;
        private readonly ILogger<UioHookService> logger;

        private readonly HashSet<ModifierKey> hotModifierKeys = new();

        private readonly Subject<ModifierKey> rawKeyPressedSubject = new();
        private readonly Subject<ModifierKey> hotKeyPressedSubject = new();
        private readonly Dictionary<ModifierKey, IDisposable> hotModifierKeyPressedSubscriptions = new();

        private readonly HashSet<KeyCode> pressedKeys = new();
        private readonly HashSet<KeyCode> releasedKeys = new();

        public UioHookService(IScheduler scheduler, ILogger<UioHookService> logger)
        {
            this.scheduler = scheduler;
            this.logger = logger;
        }

        ~UioHookService() =>
            this.Dispose();

        public IObservable<ModifierKey> HotKeyPressed =>
            this.hotKeyPressedSubject.AsObservable();

        public void Register(IEnumerable<ModifierKey> modifierKeys, int pressedCount, int waitMilliseconds)
        {
            this.ThrowIfDisposed();

            var modifierKey = modifierKeys.Flatten();

            this.logger.LogDebug($"Registering a hot key: {modifierKeys}");

            this.hotModifierKeys.Add(modifierKey);

            var subscription = pressedCount == 1
                ? this.SubscribeToKeyPressesSimple(modifierKey)
                : this.SubscribeToKeyPresses(modifierKey, pressedCount, TimeSpan.FromMilliseconds(waitMilliseconds));

            this.hotModifierKeyPressedSubscriptions.Add(modifierKey, subscription);

            this.logger.LogDebug($"Registered a hot key: {modifierKey}");
        }

        public void Unregister(IEnumerable<ModifierKey> modifierKeys)
        {
            this.ThrowIfDisposed();

            var modifierKey = modifierKeys.Flatten();

            this.logger.LogDebug($"Unregistering a hot key: {modifierKey}");

            if (!this.hotModifierKeys.Contains(modifierKey))
            {
                this.logger.LogWarning($"Modifier key {modifierKey} not found");
                return;
            }

            this.hotModifierKeys.Remove(modifierKey);

            this.hotModifierKeyPressedSubscriptions[modifierKey].Dispose();
            this.hotModifierKeyPressedSubscriptions.Remove(modifierKey);

            this.logger.LogDebug($"Unregistered a hot modifier key: {modifierKey}");
        }

        public void UnregisterAll()
        {
            this.logger.LogDebug("Unregistering all hot keys");

            this.ThrowIfDisposed();

            this.hotModifierKeys.Clear();

            this.hotModifierKeyPressedSubscriptions.Values.ForEach(subscription => subscription.Dispose());
            this.hotModifierKeyPressedSubscriptions.Clear();

            this.logger.LogDebug("Unregistered all hot keys");
        }

        public Task StartHook(CancellationToken token)
        {
            var source = new TaskCompletionSource<object?>();

            var thread = new Thread(() =>
            {
                try
                {
                    this.CreateHook(token);
                    source.SetResult(null);
                } catch (Exception e)
                {
                    source.SetException(e);
                }
            });

            thread.Start();

            return source.Task;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.taskQueue.Dispose();

                this.rawKeyPressedSubject.Dispose();
                this.hotKeyPressedSubject.Dispose();

                foreach (var subscription in this.hotModifierKeyPressedSubscriptions.Values)
                {
                    subscription.Dispose();
                }

                this.hotModifierKeyPressedSubscriptions.Clear();
            }
        }

        private IDisposable SubscribeToKeyPressesSimple(ModifierKey modifierKey) =>
            this.rawKeyPressedSubject
                .Where(key => key.IsSubsetKeyOf(modifierKey))
                .Subscribe(this.hotKeyPressedSubject);

        private IDisposable SubscribeToKeyPresses(ModifierKey modifierKey, int pressedCount, TimeSpan waitTime) =>
            this.rawKeyPressedSubject
                .Where(key => key.IsSubsetKeyOf(modifierKey))
                .Buffer(this.rawKeyPressedSubject
                    .Scan(
                        DateTimeOffset.MinValue,
                        (lastKeyPressTime, _) => this.scheduler.Now - lastKeyPressTime > waitTime
                            ? this.scheduler.Now
                            : lastKeyPressTime)
                    .Delay(waitTime, scheduler))
                .Where(modifiers => modifiers.Count == pressedCount && modifiers.All(m => m == modifiers[0]))
                .Select(modifiers => modifiers[0])
                .Subscribe(this.hotKeyPressedSubject);

        private void CreateHook(CancellationToken token)
        {
            this.logger.LogInformation("Creating a global hook");
            token.Register(() => UioHook.Stop());
            UioHook.SetDispatchProc(this.HandleHookEvent);
            UioHook.Run();
        }

        private void HandleHookEvent(ref UioHookEvent e)
        {
            if (e.Type == EventType.KeyPressed || e.Type == EventType.KeyReleased)
            {
                var copy = e;
                this.taskQueue.EnqueueAndIgnore(() => Task.Run(() => this.HandleSingleKeyboardInput(copy)));
            }
        }

        private void HandleSingleKeyboardInput(UioHookEvent e)
        {
            if (e.Type == EventType.KeyPressed)
            {
                this.HandleKeyDown((KeyCode)e.Keyboard.KeyCode);
            } else if (e.Type == EventType.KeyReleased)
            {
                this.HandleKeyUp((KeyCode)e.Keyboard.KeyCode);
            }
        }

        private void HandleKeyDown(KeyCode keyCode)
        {
            lock (this.keyLock)
            {
                this.releasedKeys.Clear();
                this.pressedKeys.Add(keyCode);
            }
        }

        private void HandleKeyUp(KeyCode keyCode)
        {
            lock (this.keyLock)
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

                var modifierKeys = this.releasedKeys
                    .Select(key => key.ToModifierKey())
                    .ToList();

                this.releasedKeys.Clear();

                if (!modifierKeys.Any(key => key is null) && modifierKeys.Count > 1)
                {
                    var modifierKey = modifierKeys.Select(key => key!.Value).Flatten();
                    this.logger.LogDebug($"Key combination activated: {modifierKey}");
                    this.rawKeyPressedSubject.OnNext(modifierKey);
                }
            }
        }
    }
}

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
        private static readonly IEqualityComparer<HashSet<ModifierKey>> SetComparer =
            HashSet<ModifierKey>.CreateSetComparer();

        private readonly TaskQueue taskQueue = new();

        private readonly IScheduler scheduler;
        private readonly ILogger<UioHookService> logger;

        private readonly HashSet<HashSet<ModifierKey>> hotModifierKeys = new(SetComparer);

        private readonly Subject<ISet<ModifierKey>> keysPressedSubject = new();
        private readonly Subject<ISet<ModifierKey>> hotKeyPressedSubject = new();
        private readonly Dictionary<HashSet<ModifierKey>, IDisposable> hotModifierKeyPressedSubscriptions =
            new(SetComparer);

        private readonly HashSet<KeyCode> pressedKeys = new();

        public UioHookService(IScheduler scheduler, ILogger<UioHookService> logger)
        {
            this.scheduler = scheduler;
            this.logger = logger;
        }

        ~UioHookService() =>
            this.Dispose();

        public IObservable<ISet<ModifierKey>> HotKeyPressed =>
            this.hotKeyPressedSubject.AsObservable();

        public void Register(IEnumerable<ModifierKey> modifierKeys, int pressedCount, int waitMilliseconds)
        {
            this.ThrowIfDisposed();

            var modifierKeysSet = modifierKeys.ToHashSet();

            this.logger.LogDebug($"Registering a hot key: {modifierKeysSet.ToFormattedString()}");

            this.hotModifierKeys.Add(modifierKeysSet);

            IDisposable subscription;

            if (pressedCount == 1)
            {
                subscription = this.keysPressedSubject
                    .Where(keys => keys.SetEquals(modifierKeysSet))
                    .Subscribe(this.hotKeyPressedSubject);
            } else
            {
                var waitTime = TimeSpan.FromMilliseconds(waitMilliseconds);

                subscription = this.keysPressedSubject
                    .Where(keys => keys.SetEquals(modifierKeysSet))
                    .Buffer(this.keysPressedSubject
                        .Scan(
                            DateTimeOffset.MinValue,
                            (lastPrimary, _) => scheduler.Now - lastPrimary > waitTime
                                ? DateTimeOffset.Now
                                : lastPrimary)
                        .Delay(waitTime, scheduler))
                    .Where(modifiers => modifiers.Count == pressedCount && modifiers.All(m => m == modifiers[0]))
                    .Select(modifiers => modifiers[0])
                    .Subscribe(this.hotKeyPressedSubject);
            }

            this.hotModifierKeyPressedSubscriptions.Add(modifierKeysSet, subscription);

            this.logger.LogDebug($"Registered a hot key: {modifierKeysSet.ToFormattedString()}");
        }

        public void Unregister(IEnumerable<ModifierKey> modifierKeys)
        {
            this.ThrowIfDisposed();

            var modifierKeysSet = modifierKeys.ToHashSet();

            this.logger.LogDebug($"Unregistering a hot key: {modifierKeysSet.ToFormattedString()}");

            if (!this.hotModifierKeys.Contains(modifierKeysSet))
            {
                this.logger.LogWarning($"Modifier key {modifierKeysSet.ToFormattedString()} not found");
                return;
            }

            this.hotModifierKeys.Remove(modifierKeysSet);

            this.hotModifierKeyPressedSubscriptions[modifierKeysSet].Dispose();
            this.hotModifierKeyPressedSubscriptions.Remove(modifierKeysSet);

            this.logger.LogDebug($"Unregistered a hot modifier key: {modifierKeysSet.ToFormattedString()}");
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

                this.keysPressedSubject.Dispose();
                this.hotKeyPressedSubject.Dispose();

                foreach (var subscription in this.hotModifierKeyPressedSubscriptions.Values)
                {
                    subscription.Dispose();
                }

                this.hotModifierKeyPressedSubscriptions.Clear();
            }
        }

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
            // do nothing for now
            this.logger.LogInformation($"Key pressed: {keyCode}");
        }

        private void HandleKeyUp(KeyCode keyCode)
        {
            // do nothing for now
            this.logger.LogInformation($"Key released: {keyCode}");
        }
    }
}

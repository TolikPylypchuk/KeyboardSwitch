using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Core.Keyboard;

using Microsoft.Extensions.Logging;

using SharpHook.Native;
using SharpHook.Reactive;

namespace KeyboardSwitch.Core.Services.Hook
{
    internal sealed class SharpHookService : Disposable, IKeyboardHookService
    {
        private DateTimeOffset lastKeyPress = DateTimeOffset.MinValue;
        private readonly TimeSpan keyPressWaitThresshold = TimeSpan.FromSeconds(3);

        private readonly IReactiveGlobalHook hook;
        private readonly IScheduler scheduler;
        private readonly ILogger<SharpHookService> logger;

        private readonly HashSet<ModifierMask> hotKeys = new();

        private readonly Subject<ModifierMask> rawModifierPressedSubject = new();
        private readonly Subject<ModifierMask> hotKeyPressedSubject = new();
        private readonly Dictionary<ModifierMask, IDisposable> hotModifierKeyPressedSubscriptions = new();

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
            this.Dispose();

        public IObservable<ModifierMask> HotKeyPressed =>
            this.hotKeyPressedSubject.AsObservable();

        public void Register(IEnumerable<ModifierMask> modifiers, int pressedCount, int waitMilliseconds)
        {
            this.ThrowIfDisposed();

            var modifier = modifiers.Flatten();

            this.logger.LogDebug($"Registering a hot key: {modifier}");

            this.hotKeys.Add(modifier);

            var subscription = pressedCount == 1
                ? this.SubscribeToKeyPressesSimple(modifier)
                : this.SubscribeToKeyPresses(modifier, pressedCount, TimeSpan.FromMilliseconds(waitMilliseconds));

            this.hotModifierKeyPressedSubscriptions.Add(modifier, subscription);

            this.logger.LogDebug($"Registered a hot key: {modifier}");
        }

        public void Unregister(IEnumerable<ModifierMask> modifierKeys)
        {
            this.ThrowIfDisposed();

            var modifierKey = modifierKeys.Flatten();

            this.logger.LogDebug($"Unregistering a hot key: {modifierKey}");

            if (!this.hotKeys.Contains(modifierKey))
            {
                this.logger.LogWarning($"Modifier key {modifierKey} not found");
                return;
            }

            this.hotKeys.Remove(modifierKey);

            this.hotModifierKeyPressedSubscriptions[modifierKey].Dispose();
            this.hotModifierKeyPressedSubscriptions.Remove(modifierKey);

            this.logger.LogDebug($"Unregistered a hot modifier key: {modifierKey}");
        }

        public void UnregisterAll()
        {
            this.logger.LogDebug("Unregistering all hot keys");

            this.ThrowIfDisposed();

            this.hotKeys.Clear();

            this.hotModifierKeyPressedSubscriptions.Values.ForEach(subscription => subscription.Dispose());
            this.hotModifierKeyPressedSubscriptions.Clear();

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
                this.hook.Dispose();
                this.rawModifierPressedSubject.Dispose();
                this.hotKeyPressedSubject.Dispose();

                foreach (var subscription in this.hotModifierKeyPressedSubscriptions.Values)
                {
                    subscription.Dispose();
                }

                this.hotModifierKeyPressedSubscriptions.Clear();
            }
        }

        private IDisposable SubscribeToKeyPressesSimple(ModifierMask modifier) =>
            this.rawModifierPressedSubject
                .Where(key => key.IsSubsetKeyOf(modifier))
                .Subscribe(this.hotKeyPressedSubject);

        private IDisposable SubscribeToKeyPresses(ModifierMask modifier, int pressedCount, TimeSpan waitTime) =>
            this.rawModifierPressedSubject
                .Where(key => key.IsSubsetKeyOf(modifier))
                .Buffer(this.rawModifierPressedSubject
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
                var modifierKey = modifiers.Select(key => key!.Value).Flatten();
                this.logger.LogDebug($"Key combination activated: {modifierKey}");
                this.rawModifierPressedSubject.OnNext(modifierKey);
            }
        }
    }
}

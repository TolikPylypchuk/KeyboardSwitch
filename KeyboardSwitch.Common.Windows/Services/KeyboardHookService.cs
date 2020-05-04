using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

using static KeyboardSwitch.Common.Windows.Interop.Native;

namespace KeyboardSwitch.Common.Windows.Services
{
    internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    internal sealed class KeyboardHookService : DisposableService, IKeyboardHookService
    {
        private IntPtr hookId = IntPtr.Zero;

        private readonly object modifiersLock = new object();
        private readonly TaskQueue taskQueue = new TaskQueue();

        private LowLevelKeyboardProc? hook;

        private readonly IKeysService keysService;
        private readonly IScheduler scheduler;
        private readonly ILogger<KeyboardHookService> logger;

        private readonly HashSet<HotKey> hotKeys = new HashSet<HotKey>();
        private readonly HashSet<ModifierKeys> hotModifierKeys = new HashSet<ModifierKeys>();

        private readonly Subject<HotKey> hotKeyPressedSubject = new Subject<HotKey>();
        private readonly Subject<ModifierKeys> hotModifierKeyPressedSubject = new Subject<ModifierKeys>();

        private ModifierKeys downModifierKeys;
        private HotKey? pressedHotKey;
        private ModifierKeys? pressedModifierKeys;

        public KeyboardHookService(
            IKeysService modiferKeysService,
            IScheduler scheduler,
            ILogger<KeyboardHookService> logger)
        {
            this.keysService = modiferKeysService;
            this.scheduler = scheduler;
            this.logger = logger;
        }

        ~KeyboardHookService()
            => this.Dispose();

        public IObservable<HotKey> HotKeyPressed
            => this.hotKeyPressedSubject.AsObservable();

        public HotKey RegisterHotKey(int virtualKeyCode)
            => this.RegisterHotKey(ModifierKeys.None, virtualKeyCode);

        public HotKey RegisterHotKey(ModifierKeys modifiers, int virtualKeyCode)
        {
            this.ThrowIfDisposed();

            var key = new HotKey(modifiers, virtualKeyCode);

            this.logger.LogDebug($"Registering a hot key: {key}");

            this.hotKeys.Add(key);

            this.logger.LogDebug($"Registered a hot key: {key}");

            return key;
        }

        public void RegisterHotModifierKey(ModifierKeys modifierKeys, int pressedCount, int waitMilliseconds)
        {
            this.ThrowIfDisposed();

            this.logger.LogDebug($"Registering a hot modifier key: {modifierKeys.ToFormattedString()}");

            this.hotModifierKeys.Add(modifierKeys);

            if (pressedCount == 1)
            {
                this.hotModifierKeyPressedSubject
                    .Where(keys => keys == modifierKeys)
                    .Select(keys => new HotKey(keys, 0))
                    .Subscribe(this.hotKeyPressedSubject);
            } else
            {
                var waitTime = TimeSpan.FromMilliseconds(waitMilliseconds);

                this.hotModifierKeyPressedSubject
                    .Where(keys => keys == modifierKeys)
                    .Select(keys => new HotKey(keys, 0))
                    .Buffer(this.hotModifierKeyPressedSubject
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

            this.logger.LogDebug($"Registered a hot modifier key: {modifierKeys.ToFormattedString()}");
        }

        public void UnregisterHotKey(ModifierKeys modifiers, int virtualKeyCode)
            => this.UnregisterHotKey(new HotKey(modifiers, virtualKeyCode));

        public void UnregisterHotKey(HotKey key)
        {
            this.ThrowIfDisposed();

            this.logger.LogDebug($"Unregistering a hot key: {key}");

            if (!this.hotKeys.Contains(key))
            {
                this.logger.LogWarning($"Key {key} not found");
                return;
            }

            this.hotKeys.Remove(key);

            this.logger.LogDebug($"Unregistered a hot key: {key}");
        }

        public void UnregisterHotModifierKey(ModifierKeys modifierKeys)
        {
            this.ThrowIfDisposed();

            this.logger.LogDebug($"Unregistering a hot modifier key: {modifierKeys.ToFormattedString()}");

            if (!this.hotModifierKeys.Contains(modifierKeys))
            {
                this.logger.LogWarning($"Modifier key {modifierKeys.ToFormattedString()} not found");
                return;
            }

            this.hotModifierKeys.Remove(modifierKeys);

            this.logger.LogDebug($"Unregistered a hot modifier key: {modifierKeys.ToFormattedString()}");
        }

        public void UnregisterAll()
        {
            this.logger.LogDebug("Unregistering all hot keys");

            this.ThrowIfDisposed();

            this.hotKeys.ForEach(this.UnregisterHotKey);
            this.hotModifierKeys.ForEach(this.UnregisterHotModifierKey);
        }

        public Task WaitForMessagesAsync(CancellationToken token)
            => Task.Run(() =>
                {
                    this.CreateHook();

                    this.logger.LogInformation("Starting the message loop to check for keyboard input");

                    while (GetMessage(out var msg, IntPtr.Zero, 0, 0) && !token.IsCancellationRequested)
                    {
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }
                },
                token);

        public void Dispose()
        {
            if (!this.Disposed)
            {
                UnhookWindowsHookEx(hookId);
                GC.SuppressFinalize(this);

                this.Disposed = true;
            }
        }

        private void CreateHook()
        {
            this.logger.LogInformation("Creating a global hook");

            var hMod = Marshal.GetHINSTANCE(typeof(KeyboardHookService).Module);

            this.hook = this.OnMessageReceived;
            this.hookId = SetWindowsHookEx(WhKeyboardLL, this.hook, hMod, 0);
        }

        private IntPtr OnMessageReceived(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                this.taskQueue.EnqueueAndIgnore(() => Task.Run(() => this.HandleSingleKeyboardInput(wParam, vkCode)));
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private void HandleSingleKeyboardInput(IntPtr wParam, int vkCode)
        {
            var modifierKey = this.keysService.GetModifierKeyFromCode(vkCode);

            if (wParam == WmKeyDown || wParam == WmSysKeyDown)
            {
                this.HandleKeyDown(modifierKey, vkCode);
            }

            if (wParam == WmKeyUp || wParam == WmSysKeyUp)
            {
                this.HandleKeyUp(modifierKey);
            }
        }

        private void HandleKeyDown(ModifierKeys? modifierKey, int vkCode)
        {
            if (modifierKey != null)
            {
                lock (this.modifiersLock)
                {
                    this.downModifierKeys |= modifierKey.Value;
                }
            }

            if (this.pressedHotKey == null)
            {
                var currentKey = new HotKey(this.downModifierKeys, vkCode);

                if (this.hotKeys.Contains(currentKey))
                {
                    this.pressedHotKey = currentKey;
                }
            }

            if (this.hotModifierKeys.Contains(this.downModifierKeys))
            {
                this.pressedModifierKeys = this.downModifierKeys;
            }

            if (!this.hotModifierKeys.Contains(this.downModifierKeys) || modifierKey == null)
            {
                this.pressedModifierKeys = null;
            }
        }

        private void HandleKeyUp(ModifierKeys? modifierKey)
        {
            if (modifierKey != null)
            {
                lock (this.modifiersLock)
                {
                    this.downModifierKeys &= ~modifierKey.Value;
                }
            }

            if (this.downModifierKeys == ModifierKeys.None)
            {
                if (this.pressedHotKey != null)
                {
                    var hotKey = this.pressedHotKey;
                    this.pressedHotKey = null;

                    this.hotKeyPressedSubject.OnNext(hotKey);
                }

                if (this.pressedModifierKeys != null)
                {
                    var modifierKeys = this.pressedModifierKeys.Value;
                    this.pressedModifierKeys = null;

                    this.hotModifierKeyPressedSubject.OnNext(modifierKeys);
                }
            }
        }
    }
}

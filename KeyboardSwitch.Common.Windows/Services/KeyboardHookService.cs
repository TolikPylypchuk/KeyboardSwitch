using System;
using System.Collections.Generic;
using System.Linq;
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
        private LowLevelKeyboardProc? hook;

        private readonly IKeysService keysService;
        private readonly ILogger<KeyboardHookService> logger;

        private readonly HashSet<HotKey> hotKeys = new HashSet<HotKey>();

        private readonly Subject<HotKey> hotKeyPressedSubject = new Subject<HotKey>();
        
        private ModifierKeys downModifierKeys;
        private HotKey? pressedHotKey;

        public KeyboardHookService(IKeysService modiferKeysService, ILogger<KeyboardHookService> logger)
        {
            this.keysService = modiferKeysService;
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

        public void UnregisterHotKey(int virtualKeyCode)
            => this.UnregisterHotKey(ModifierKeys.None, virtualKeyCode);

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

        public void UnregisterAll()
        {
            this.logger.LogDebug("Unregistering all hot keys");

            this.ThrowIfDisposed();
            this.hotKeys.ForEach(this.UnregisterHotKey);
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
                Task.Run(() => this.HandleSingleKeyboardInput(wParam, vkCode));
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private void HandleSingleKeyboardInput(IntPtr wParam, int vkCode)
        {
            var modifierKey = this.keysService.GetModifierKeyFromCode(vkCode);

            if (wParam == WmKeyDown || wParam == WmSysKeyDown)
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
            }

            if (wParam == WmKeyUp || wParam == WmSysKeyUp)
            {
                if (modifierKey != null)
                {
                    lock (this.modifiersLock)
                    {
                        this.downModifierKeys &= ~modifierKey.Value;
                    }
                }

                if (this.downModifierKeys == ModifierKeys.None && this.pressedHotKey != null)
                {
                    var hotKey = this.pressedHotKey;
                    this.pressedHotKey = null;

                    this.hotKeyPressedSubject.OnNext(hotKey);
                }
            }
        }
    }
}

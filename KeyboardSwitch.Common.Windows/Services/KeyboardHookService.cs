using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

using static KeyboardSwitch.Common.Windows.Interop.Native;

namespace KeyboardSwitch.Common.Windows.Services
{
    internal delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    public sealed class KeyboardHookService : IKeyboardHookService
    {
        private IntPtr hookId = IntPtr.Zero;

        private readonly object modifiersLock = new object();
        private LowLevelKeyboardProc? hook;

        private readonly IModiferKeysService modiferKeysService;
        private readonly ILogger<KeyboardHookService> logger;

        private readonly Dictionary<HotKey, Action<HotKey>> hotKeyActions = new Dictionary<HotKey, Action<HotKey>>();
        private ModifierKeys downModifierKeys;

        private Action? scheduledAction;


        private bool disposed = false;

        public KeyboardHookService(IModiferKeysService modiferKeysService, ILogger<KeyboardHookService> logger)
        {
            this.modiferKeysService = modiferKeysService;
            this.logger = logger;
        }

        ~KeyboardHookService()
            => this.Dispose();

        public HotKey RegisterHotKey(int virtualKeyCode, Action<HotKey> action)
            => this.RegisterHotKey(ModifierKeys.None, virtualKeyCode, action);

        public HotKey RegisterHotKey(ModifierKeys modifiers, int virtualKeyCode, Action<HotKey> action)
        {
            this.ThrowIfDisposed();

            var key = new HotKey(modifiers, virtualKeyCode);

            this.logger.LogTrace($"Registering a hot key: {key}");

            this.hotKeyActions.Add(key, action);

            this.logger.LogTrace($"Registered a hot key: {key}");

            return key;
        }

        public void UnregisterHotKey(int virtualKeyCode)
            => this.UnregisterHotKey(ModifierKeys.None, virtualKeyCode);

        public void UnregisterHotKey(ModifierKeys modifiers, int virtualKeyCode)
            => this.UnregisterHotKey(new HotKey(modifiers, virtualKeyCode));

        public void UnregisterHotKey(HotKey key)
        {
            this.ThrowIfDisposed();

            this.logger.LogTrace($"Unregistering a hot key: {key}");

            if (!this.hotKeyActions.ContainsKey(key))
            {
                this.logger.LogWarning($"Key {key} not found");
                return;
            }

            this.hotKeyActions.Remove(key);

            this.logger.LogTrace($"Unregistered a hot key: {key}");
        }

        public void UnregisterAll()
        {
            this.logger.LogTrace("Unregistering all hot keys");

            this.ThrowIfDisposed();
            this.hotKeyActions.Keys.ForEach(this.UnregisterHotKey);
        }

        public Task WaitForMessagesAsync(CancellationToken token)
            => Task.Run(() =>
                {
                    this.CreateHook();

                    this.logger.LogTrace("Starting the message loop to check for keyboard input");

                    while (GetMessage(out var msg, IntPtr.Zero, 0, 0) && !token.IsCancellationRequested)
                    {
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }
                },
                token);

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.logger.LogTrace("Destroying the global hook");

                UnhookWindowsHookEx(hookId);
                GC.SuppressFinalize(this);

                this.disposed = true;
            }
        }

        private void CreateHook()
        {
            this.logger.LogTrace("Creating a global hook");

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
            var modifierKey = this.modiferKeysService.GetModifierKeyFromCode(vkCode);

            if (wParam == (IntPtr)WmKeyDown || wParam == (IntPtr)WmSysKeyDown)
            {
                if (modifierKey != null)
                {
                    lock (this.modifiersLock)
                    {
                        this.downModifierKeys |= modifierKey.Value;
                    }
                }
            }

            if (wParam == (IntPtr)WmKeyUp || wParam == (IntPtr)WmSysKeyUp)
            {
                if (modifierKey != null)
                {
                    lock (this.modifiersLock)
                    {
                        this.downModifierKeys &= ~modifierKey.Value;
                    }
                }

                if (this.downModifierKeys == ModifierKeys.None && this.scheduledAction != null)
                {
                    this.scheduledAction();
                    this.scheduledAction = null;
                } else
                {
                    var currentKey = new HotKey(this.downModifierKeys, vkCode);

                    if (this.hotKeyActions.TryGetValue(currentKey, out var action))
                    {
                        this.scheduledAction = () => action(currentKey);
                    }
                }
            }
        }

        private void ThrowIfDisposed([CallerMemberName] string? method = null)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException($"Cannot call {method} - the service is disposed");
            }
        }
    }
}

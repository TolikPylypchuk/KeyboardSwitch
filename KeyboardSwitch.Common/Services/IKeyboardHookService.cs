using System;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
{
    public interface IKeyboardHookService : IDisposable
    {
        IObservable<HotKey> HotKeyPressed { get; }

        HotKey RegisterHotKey(int virtualKeyCode);
        HotKey RegisterHotKey(ModifierKeys modifiers, int virtualKeyCode);
        void RegisterHotModifierKey(ModifierKeys modifierKeys, int pressedCount, int waitMilliseconds);

        void UnregisterHotKey(ModifierKeys modifiers, int virtualKeyCode);
        void UnregisterHotKey(HotKey key);
        void UnregisterHotModifierKey(ModifierKeys modifierKeys);

        void UnregisterAll();

        Task WaitForMessagesAsync(CancellationToken token);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeyboardSwitch.Common.Services
{
    public interface IKeyboardHookService : IDisposable
    {
        IObservable<HotKey> HotKeyPressed { get; }

        HotKey RegisterHotKey(int virtualKeyCode);
        HotKey RegisterHotKey(ModifierKeys modifiers, int virtualKeyCode);

        void UnregisterHotKey(int virtualKeyCode);
        void UnregisterHotKey(ModifierKeys modifiers, int virtualKeyCode);
        void UnregisterHotKey(HotKey key);

        void UnregisterAll();

        Task WaitForMessagesAsync(CancellationToken token);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace KeyboardSwitch.Common.Services
{
    public interface IKeyboardHookService : IDisposable
    {
        HotKey RegisterHotKey(int virtualKeyCode, Action<HotKey> action);
        HotKey RegisterHotKey(ModifierKeys modifiers, int virtualKeyCode, Action<HotKey> action);

        void UnregisterHotKey(int virtualKeyCode);
        void UnregisterHotKey(ModifierKeys modifiers, int virtualKeyCode);
        void UnregisterHotKey(HotKey key);

        void UnregisterAll();

        Task WaitForMessagesAsync(CancellationToken token);
    }
}

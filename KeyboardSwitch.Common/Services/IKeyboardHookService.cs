using System;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
{
    public interface IKeyboardHookService : IDisposable
    {
        IObservable<HotKey> HotKeyPressed { get; }

        void Register(ModifierKeys modifierKeys, int pressedCount, int waitMilliseconds);
        void Unregister(ModifierKeys modifierKeys);

        void UnregisterAll();

        Task StartHook(CancellationToken token);
    }
}

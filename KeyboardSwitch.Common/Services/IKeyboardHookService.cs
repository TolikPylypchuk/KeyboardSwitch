using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
{
    public interface IKeyboardHookService : IDisposable
    {
        IObservable<ModifierKey> HotKeyPressed { get; }

        void Register(IEnumerable<ModifierKey> modifierKeys, int pressedCount, int waitMilliseconds);
        void Unregister(IEnumerable<ModifierKey> modifierKeys);

        void UnregisterAll();

        Task StartHook(CancellationToken token);
    }
}

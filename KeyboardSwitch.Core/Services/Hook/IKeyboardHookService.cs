using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using SharpHook.Native;

namespace KeyboardSwitch.Core.Services.Hook
{
    public interface IKeyboardHookService : IDisposable
    {
        IObservable<ModifierMask> HotKeyPressed { get; }

        void Register(IEnumerable<ModifierMask> modifiers, int pressedCount, int waitMilliseconds);
        void Unregister(IEnumerable<ModifierMask> modifiers);

        void UnregisterAll();

        Task StartHook(CancellationToken token);
    }
}

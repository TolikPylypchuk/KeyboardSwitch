using System;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public interface IForm : IReactiveObject
    {
        IObservable<bool> FormChanged { get; }
        bool IsFormChanged { get; }

        IObservable<bool> Valid { get; }
    }
}

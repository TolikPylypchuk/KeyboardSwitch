namespace KeyboardSwitch.Settings.Core.ViewModels;

public interface IReactiveForm : IReactiveObject
{
    IObservable<bool> FormChanged { get; }

    IObservable<bool> Valid { get; }
}

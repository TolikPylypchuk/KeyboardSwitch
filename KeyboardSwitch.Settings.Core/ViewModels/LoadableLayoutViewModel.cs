namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed class LoadableLayoutViewModel(LoadableKeyboardLayout layout) : ReactiveObject
{
    public LoadableKeyboardLayout Layout { get; } = layout;

    public ReactiveCommand<Unit, Unit> Delete { get; } = ReactiveCommand.Create(() => { });
}

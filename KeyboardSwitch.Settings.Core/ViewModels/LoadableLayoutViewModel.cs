using System.Reactive;

using KeyboardSwitch.Common.Keyboard;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class LoadableLayoutViewModel : ReactiveObject
    {
        public LoadableLayoutViewModel(LoadableKeyboardLayout layout)
        {
            this.Layout = layout;
            this.Delete = ReactiveCommand.Create(() => { });
        }

        public LoadableKeyboardLayout Layout { get; }

        public ReactiveCommand<Unit, Unit> Delete { get; }
    }
}

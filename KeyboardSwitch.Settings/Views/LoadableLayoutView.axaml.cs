using System.Reactive.Disposables;

using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public partial class LoadableLayoutView : ReactiveUserControl<LoadableLayoutViewModel>
    {
        public LoadableLayoutView()
        {
            this.InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.NameTextBlock.Text = this.ViewModel!.Layout.Name;

                this.BindCommand(this.ViewModel, vm => vm.Delete, v => v.DeleteButton)
                    .DisposeWith(disposables);
            });
        }
    }
}

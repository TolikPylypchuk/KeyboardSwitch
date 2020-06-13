using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class LoadableLayoutView : ReactiveUserControl<LoadableLayoutViewModel>
    {
        public LoadableLayoutView()
        {
            this.WhenActivated(disposables =>
            {
                this.NameTextBlock.Text = this.ViewModel.Layout.Name;

                this.BindCommand(this.ViewModel, vm => vm.Delete, v => v.DeleteButton)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TextBlock NameTextBlock { get; set; } = null!;
        private Button DeleteButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.NameTextBlock = this.FindControl<TextBlock>(nameof(this.NameTextBlock));
            this.DeleteButton = this.FindControl<Button>(nameof(this.DeleteButton));
        }
    }
}

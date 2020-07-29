using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class CharMappingView : ReactiveUserControl<CharMappingViewModel>
    {
        public CharMappingView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.Layouts, v => v.Layouts.Items)
                    ?.DisposeWith(disposables);

                this.BindCommands(disposables);
                this.BindTextBlocks(disposables);
            });

            this.InitializeComponent();
        }

        private ItemsControl Layouts { get; set; } = null!;
        private Button AutoConfigureButton { get; set; } = null!;

        private TextBlock NewLayoutsTextBlock { get; set; } = null!;
        private StackPanel RemoveLayoutsPanel { get; set; } = null!;
        private Button RemoveLayoutsButton { get; set; } = null!;

        private StackPanel ActionPanel { get; set; } = null!;
        private Button SaveButton { get; set; } = null!;
        private Button CancelButton { get; set; } = null!;

        private TextBlock RestartServiceTextBlock { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.Layouts = this.FindControl<ItemsControl>(nameof(Layouts));
            this.AutoConfigureButton = this.FindControl<Button>(nameof(this.AutoConfigureButton));

            this.NewLayoutsTextBlock = this.Find<TextBlock>(nameof(this.NewLayoutsTextBlock));
            this.RemoveLayoutsPanel = this.Find<StackPanel>(nameof(this.RemoveLayoutsPanel));
            this.RemoveLayoutsButton = this.Find<Button>(nameof(this.RemoveLayoutsButton));

            this.ActionPanel = this.FindControl<StackPanel>(nameof(ActionPanel));
            this.SaveButton = this.FindControl<Button>(nameof(this.SaveButton));
            this.CancelButton = this.FindControl<Button>(nameof(this.CancelButton));

            this.RestartServiceTextBlock = this.Find<TextBlock>(nameof(this.RestartServiceTextBlock));
        }

        private void BindCommands(CompositeDisposable disposables)
        {
            this.BindCommand(this.ViewModel, vm => vm.AutoConfigure, v => v.AutoConfigureButton)
                    .DisposeWith(disposables);

            this.ViewModel.AutoConfigure.CanExecute
                .BindTo(this, v => v.AutoConfigureButton.IsVisible)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.RemoveLayouts, v => v.RemoveLayoutsButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                .DisposeWith(disposables);

            Observable.CombineLatest(this.ViewModel.Save.CanExecute, this.ViewModel.Cancel.CanExecute)
                .AnyTrue()
                .BindTo(this, v => v.ActionPanel.IsVisible)
                .DisposeWith(disposables);
        }

        private void BindTextBlocks(CompositeDisposable disposables)
        {
            this.WhenAnyValue(v => v.ViewModel.HasNewLayouts)
                .BindTo(this, v => v.NewLayoutsTextBlock.IsVisible)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel.CanRemoveLayouts)
                .BindTo(this, v => v.RemoveLayoutsPanel.IsVisible)
                .DisposeWith(disposables);

            Observable.CombineLatest(
                this.WhenAnyValue(v => v.ViewModel.HasNewLayouts),
                this.WhenAnyValue(v => v.ViewModel.ShouldRemoveLayouts))
                .AnyTrue()
                .BindTo(this, v => v.RestartServiceTextBlock.IsVisible)
                .DisposeWith(disposables);
        }
    }
}

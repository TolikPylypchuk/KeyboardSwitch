using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

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
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.AutoConfigure, v => v.AutoConfigureButton)
                    .DisposeWith(disposables);

                this.ViewModel.AutoConfigure.CanExecute
                    .BindTo(this, v => v.AutoConfigureButton.IsVisible)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                    .DisposeWith(disposables);

                this.ViewModel.Cancel.CanExecute
                    .BindTo(this, v => v.ActionPanel.IsVisible)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ItemsControl Layouts { get; set; } = null!;
        private Button AutoConfigureButton { get; set; } = null!;

        private StackPanel ActionPanel { get; set; } = null!;
        private Button SaveButton { get; set; } = null!;
        private Button CancelButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.Layouts = this.FindControl<ItemsControl>(nameof(Layouts));
            this.AutoConfigureButton = this.FindControl<Button>(nameof(this.AutoConfigureButton));

            this.ActionPanel = this.FindControl<StackPanel>(nameof(ActionPanel));
            this.SaveButton = this.FindControl<Button>(nameof(this.SaveButton));
            this.CancelButton = this.FindControl<Button>(nameof(this.CancelButton));
        }
    }
}

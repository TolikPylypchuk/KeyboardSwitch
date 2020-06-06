using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class ConverterSettingsView : ReactiveUserControl<ConverterSettingsViewModel>
    {
        public ConverterSettingsView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.CustomLayouts, v => v.Layouts.Items)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.AddCustomLayout, v => v.AddLayoutButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                    .DisposeWith(disposables);

                this.WhenAnyObservable(v => v.ViewModel.Cancel.CanExecute)
                    .BindTo(this, v => v.ActionPanel.IsVisible)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ItemsControl Layouts { get; set; } = null!;
        private Button AddLayoutButton { get; set; } = null!;

        private StackPanel ActionPanel { get; set; } = null!;
        private Button SaveButton { get; set; } = null!;
        private Button CancelButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.Layouts = this.FindControl<ItemsControl>(nameof(this.Layouts));
            this.AddLayoutButton = this.FindControl<Button>(nameof(this.AddLayoutButton));

            this.ActionPanel = this.FindControl<StackPanel>(nameof(this.ActionPanel));
            this.SaveButton = this.FindControl<Button>(nameof(this.SaveButton));
            this.CancelButton = this.FindControl<Button>(nameof(this.CancelButton));
        }
    }
}

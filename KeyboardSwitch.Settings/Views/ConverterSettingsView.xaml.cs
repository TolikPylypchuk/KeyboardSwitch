using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public class ConverterSettingsView : ReactiveUserControl<ConverterSettingsViewModel>
    {
        public ConverterSettingsView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.CustomLayouts, v => v.Layouts.Items)
                    .DisposeWith(disposables);

                this.BindValidation(
                    this.ViewModel, vm => vm.LayoutNamesAreUniqueRule, v => v.CustomLayoutsValidationTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.AddCustomLayout, v => v.AddLayoutButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.AutoConfigureCustomLayouts, v => v.AutoConfigureButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                    .DisposeWith(disposables);

                this.ViewModel.Cancel.CanExecute
                    .BindTo(this, v => v.ActionPanel.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.IsAutoConfiguringLayouts)
                    .Invert()
                    .BindTo(this, v => v.MainPanel.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.IsAutoConfiguringLayouts)
                    .BindTo(this, v => v.AutoConfigurationPanel.IsVisible)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ItemsControl Layouts { get; set; } = null!;
        private Button AddLayoutButton { get; set; } = null!;
        private Button AutoConfigureButton { get; set; } = null!;

        private TextBlock CustomLayoutsValidationTextBlock { get; set; } = null!;

        private StackPanel ActionPanel { get; set; } = null!;
        private Button SaveButton { get; set; } = null!;
        private Button CancelButton { get; set; } = null!;

        private DockPanel MainPanel { get; set; } = null!;
        private DockPanel AutoConfigurationPanel { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.Layouts = this.FindControl<ItemsControl>(nameof(this.Layouts));
            this.AddLayoutButton = this.FindControl<Button>(nameof(this.AddLayoutButton));
            this.AutoConfigureButton = this.FindControl<Button>(nameof(this.AutoConfigureButton));

            this.CustomLayoutsValidationTextBlock = this.FindControl<TextBlock>(
                nameof(this.CustomLayoutsValidationTextBlock));

            this.ActionPanel = this.FindControl<StackPanel>(nameof(this.ActionPanel));
            this.SaveButton = this.FindControl<Button>(nameof(this.SaveButton));
            this.CancelButton = this.FindControl<Button>(nameof(this.CancelButton));

            this.MainPanel = this.FindControl<DockPanel>(nameof(this.MainPanel));
            this.AutoConfigurationPanel = this.FindControl<DockPanel>(nameof(this.AutoConfigurationPanel));
        }
    }
}

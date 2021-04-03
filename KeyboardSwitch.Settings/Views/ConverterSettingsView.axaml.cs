using System.Reactive.Disposables;

using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public partial class ConverterSettingsView : ReactiveUserControl<ConverterSettingsViewModel>
    {
        public ConverterSettingsView()
        {
            this.InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.CustomLayouts, v => v.Layouts.Items)
                    .DisposeWith(disposables);

                this.BindValidation(
                    this.ViewModel, vm => vm!.LayoutNamesAreUniqueRule, v => v.CustomLayoutsValidationTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.AddCustomLayout, v => v.AddLayoutButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.AutoConfigureCustomLayouts, v => v.AutoConfigureButton)
                    .DisposeWith(disposables);

                this.ViewModel.AutoConfigureCustomLayouts.CanExecute
                    .BindTo(this, v => v.AutoConfigureButton.IsVisible)
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
                    .BindTo(this, v => v.LoadableLayoutsControl.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.LoadableLayoutsSettingsViewModel)
                    .WhereNotNull()
                    .BindTo(this, v => v.LoadableLayoutsControl.Content)
                    .DisposeWith(disposables);
            });
        }
    }
}

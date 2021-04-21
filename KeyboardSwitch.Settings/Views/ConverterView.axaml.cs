using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;

using KeyboardSwitch.Core;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public partial class ConverterView : ReactiveUserControl<ConverterViewModel>
    {
        public ConverterView()
        {
            this.InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.BindConverterVisibility(disposables);
                this.BindControls(disposables);
                this.BindCommands(disposables);
            });
        }

        private void BindConverterVisibility(CompositeDisposable disposables)
        {
            var enoughLayouts = this.ViewModel!.Layouts
                .ToObservableChangeSet()
                .Count()
                .StartWith(this.ViewModel.Layouts.Count)
                .Select(count => count >= 2);

            enoughLayouts.BindTo(this, v => v.ConverterGrid.IsVisible)
                .DisposeWith(disposables);

            enoughLayouts.Invert()
                .BindTo(this, v => v.TooFewLayoutsTextBlock.IsVisible)
                .DisposeWith(disposables);
        }

        private void BindControls(CompositeDisposable disposables)
        {
            this.Bind(this.ViewModel, vm => vm.SourceText, v => v.SourceTextBox.Text)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.TargetText, v => v.TargetTextBox.Text)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.Layouts, v => v.SourceLayoutComboBox.Items)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.Layouts, v => v.TargetLayoutComboBox.Items)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.SourceLayout, v => v.SourceLayoutComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.TargetLayout, v => v.TargetLayoutComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.ViewModel!.Layouts
                .ToObservableChangeSet()
                .ToCollection()
                .Where(layouts => layouts.Count >= 2)
                .Subscribe(layouts =>
                {
                    this.SourceLayoutComboBox.SelectedItem = layouts.First();
                    this.TargetLayoutComboBox.SelectedItem = layouts.Skip(1).First();
                })
                .DisposeWith(disposables);

            this.BindValidation(
                this.ViewModel, vm => vm!.LayoutsAreDifferentRule, v => v.LayoutsValidationTextBlock.Text)
                .DisposeWith(disposables);
        }

        private void BindCommands(CompositeDisposable disposables)
        {
            this.BindCommand(this.ViewModel!, vm => vm.SwapLayouts, v => v.SwapButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel!, vm => vm.Convert, v => v.ConvertButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel!, vm => vm.Clear, v => v.ClearButton)
                .DisposeWith(disposables);

            this.GetObservable(KeyDownEvent, RoutingStrategies.Tunnel)
                .Where(e => e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.Control)
                .Do(e => e.Handled = true)
                .Discard()
                .InvokeCommand(this.ViewModel!.Convert)
                .DisposeWith(disposables);

            this.GetObservable(KeyDownEvent, RoutingStrategies.Tunnel)
                .Where(e => e.Key == Key.Delete && e.KeyModifiers == KeyModifiers.Control)
                .Do(e => e.Handled = true)
                .Discard()
                .InvokeCommand(this.ViewModel.Clear)
                .DisposeWith(disposables);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Settings.Converters;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public partial class PreferencesView : ReactiveUserControl<PreferencesViewModel>
    {
        public PreferencesView()
        {
            this.InitializeComponent();

            var allModifiers = this.AllModifierKeys();
            this.ForwardComboBox.Items = allModifiers;
            this.BackwardComboBox.Items = allModifiers;

            this.WhenActivated(disposables =>
            {
                this.BindControls(disposables);
                this.BindValidations(disposables);
                this.BindCommands(disposables);
            });
        }

        private void BindControls(CompositeDisposable disposables)
        {
            this.Bind(this.ViewModel, vm => vm.InstantSwitching, v => v.InstantSwitchingCheckBox.IsChecked)
                    .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.SwitchLayout, v => v.SwitchLayoutCheckBox.IsChecked)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.Startup, v => v.StartupCheckBox.IsChecked)
                .DisposeWith(disposables);

            this.Bind(
                this.ViewModel,
                vm => vm.ShowUninstalledLayoutsMessage,
                v => v.ShowRemovedLayoutsMessageCheckBox.IsChecked)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.ForwardModifierKeys, v => v.ForwardComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.BackwardModifierKeys, v => v.BackwardComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.PressCount, v => v.PressCountUpDown.Value)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.WaitMilliseconds, v => v.WaitMillisecondsUpDown.Value)
                .DisposeWith(disposables);
        }

        private void BindValidations(CompositeDisposable disposables)
        {
            this.BindValidation(
                this.ViewModel, vm => vm.PressCount, v => v.PressCountValidationTextBlock.Text)
                .DisposeWith(disposables);

            this.BindValidation(
                this.ViewModel, vm => vm.WaitMilliseconds, v => v.WaitMillisecondsValidationTextBlock.Text)
                .DisposeWith(disposables);

            this.BindValidation(
                this.ViewModel,
                vm => vm!.SwitchMethodsAreDifferentRule,
                v => v.SwitchMethodsValidationTextBlock.Text)
                .DisposeWith(disposables);
        }

        private void BindCommands(CompositeDisposable disposables)
        {
            this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                .DisposeWith(disposables);

            this.ViewModel.Cancel.CanExecute
                .BindTo(this, v => v.ActionPanel.IsVisible)
                .DisposeWith(disposables);
        }

        private List<string> AllModifierKeys() =>
            new List<ModifierKeys> { ModifierKeys.Alt, ModifierKeys.Ctrl, ModifierKeys.Shift }
                .GetPowerSet()
                .Select(modifiers => modifiers.ToList())
                .OrderBy(modifiers => modifiers.Count)
                .Skip(1)
                .Select(modifiers => modifiers.Flatten())
                .Select(Convert.ModifierKeysToString)
                .Where(value => value != null)
                .ToList();
    }
}

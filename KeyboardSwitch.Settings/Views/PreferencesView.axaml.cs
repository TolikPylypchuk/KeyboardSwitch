using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;

using Avalonia.ReactiveUI;

using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

using Convert = KeyboardSwitch.Settings.Converters.Convert;

namespace KeyboardSwitch.Settings.Views
{
    public partial class PreferencesView : ReactiveUserControl<PreferencesViewModel>
    {
        public PreferencesView()
        {
            this.InitializeComponent();

            var modifiers = this.Modifiers()
                .Where(key => key != ModifierKey.None)
                .Select(Convert.ModifierKeyToString)
                .ToImmutableList();

            var allModifiers = modifiers.Insert(0, Convert.ModifierKeyToString(ModifierKey.None));

            this.ForwardFirstComboBox.Items = modifiers;
            this.ForwardSecondComboBox.Items = modifiers;
            this.ForwardThirdComboBox.Items = allModifiers;

            this.BackwardFirstComboBox.Items = modifiers;
            this.BackwardSecondComboBox.Items = modifiers;
            this.BackwardThirdComboBox.Items = allModifiers;

            this.WhenActivated(disposables =>
            {
                this.BindCheckboxes(disposables);
                this.BindControls(disposables);
                this.BindValidations(disposables);
                this.BindCommands(disposables);
            });
        }

        private void BindCheckboxes(CompositeDisposable disposables)
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
        }

        private void BindControls(CompositeDisposable disposables)
        {
            this.Bind(this.ViewModel, vm => vm.ForwardModifierKeyFirst, v => v.ForwardFirstComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.ForwardModifierKeySecond, v => v.ForwardSecondComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.ForwardModifierKeyThird, v => v.ForwardThirdComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.BackwardModifierKeyFirst, v => v.BackwardFirstComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.BackwardModifierKeySecond, v => v.BackwardSecondComboBox.SelectedItem)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.BackwardModifierKeyThird, v => v.BackwardThirdComboBox.SelectedItem)
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
                this.ViewModel, vm => vm!.ModifierKeysAreDifferentRule, v => v.ModifierKeysValidationTextBlock.Text)
                .DisposeWith(disposables);

            this.BindValidation(
                this.ViewModel, vm => vm!.SwitchMethodsAreDifferentRule, v => v.SwitchMethodsValidationTextBlock.Text)
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

        private List<ModifierKey> Modifiers() =>
            new()
            {
                ModifierKey.None,
                ModifierKey.Ctrl,
                ModifierKey.Shift,
                ModifierKey.Alt,
                ModifierKey.Meta,
                ModifierKey.LeftCtrl,
                ModifierKey.LeftShift,
                ModifierKey.LeftAlt,
                ModifierKey.LeftMeta,
                ModifierKey.RightCtrl,
                ModifierKey.RightShift,
                ModifierKey.RightAlt,
                ModifierKey.RightMeta
            };
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Settings.Converters;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public partial class ModifierKeysSwitchView : ReactiveUserControl<ModifierKeysSwitchViewModel>
    {
        public ModifierKeysSwitchView()
        {
            this.InitializeComponent();
            var allModifiers = new List<ModifierKeys> { ModifierKeys.Alt, ModifierKeys.Ctrl, ModifierKeys.Shift }
                .GetPowerSet()
                .Select(modifiers => modifiers.ToList())
                .OrderBy(modifiers => modifiers.Count)
                .Skip(1)
                .Select(modifiers => modifiers.Flatten())
                .Select(Convert.ModifierKeysToString)
                .Where(value => value != null)
                .ToList();

            this.ForwardComboBox.Items = allModifiers;
            this.BackwardComboBox.Items = allModifiers;
            this.WhenActivated(disposables =>
            {
                this.Bind(this.ViewModel, vm => vm.ForwardModifierKeys, v => v.ForwardComboBox.SelectedItem)
                    .DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.BackwardModifierKeys, v => v.BackwardComboBox.SelectedItem)
                    .DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.PressCount, v => v.PressCountUpDown.Value)
                    .DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.WaitMilliseconds, v => v.WaitMillisecondsUpDown.Value)
                    .DisposeWith(disposables);

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
            });
        }
    }
}

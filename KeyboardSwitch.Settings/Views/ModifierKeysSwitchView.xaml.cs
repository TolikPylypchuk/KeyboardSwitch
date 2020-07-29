using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Settings.Converters;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public class ModifierKeysSwitchView : ReactiveUserControl<ModifierKeysSwitchViewModel>
    {
        public ModifierKeysSwitchView()
        {
            this.WhenActivated(disposables =>
            {
                this.Bind(this.ViewModel, vm => vm.ForwardModifierKeys, v => v.ForwardComboBox.SelectedItem)
                    ?.DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.BackwardModifierKeys, v => v.BackwardComboBox.SelectedItem)
                    ?.DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.PressCount, v => v.PressCountUpDown.Value)
                    ?.DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.WaitMilliseconds, v => v.WaitMillisecondsUpDown.Value)
                    ?.DisposeWith(disposables);

                this.BindValidation(
                        this.ViewModel, vm => vm.PressCount, v => v.PressCountValidationTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                        this.ViewModel, vm => vm.WaitMilliseconds, v => v.WaitMillisecondsValidationTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                        this.ViewModel,
                        vm => vm.SwitchMethodsAreDifferentRule,
                        v => v.SwitchMethodsValidationTextBlock.Text)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ComboBox ForwardComboBox { get; set; } = null!;
        private ComboBox BackwardComboBox { get; set; } = null!;

        private NumericUpDown PressCountUpDown { get; set; } = null!;
        private NumericUpDown WaitMillisecondsUpDown { get; set; } = null!;

        private TextBlock PressCountValidationTextBlock { get; set; } = null!;
        private TextBlock WaitMillisecondsValidationTextBlock { get; set; } = null!;
        private TextBlock SwitchMethodsValidationTextBlock { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.ForwardComboBox = this.FindControl<ComboBox>(nameof(this.ForwardComboBox));
            this.BackwardComboBox = this.FindControl<ComboBox>(nameof(this.BackwardComboBox));

            this.PressCountUpDown = this.FindControl<NumericUpDown>(nameof(this.PressCountUpDown));
            this.WaitMillisecondsUpDown = this.FindControl<NumericUpDown>(nameof(this.WaitMillisecondsUpDown));

            this.PressCountValidationTextBlock = this.FindControl<TextBlock>(
                nameof(this.PressCountValidationTextBlock));

            this.WaitMillisecondsValidationTextBlock = this.FindControl<TextBlock>(
                nameof(this.WaitMillisecondsValidationTextBlock));

            this.SwitchMethodsValidationTextBlock = this.FindControl<TextBlock>(
                nameof(this.SwitchMethodsValidationTextBlock));

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
        }
    }
}

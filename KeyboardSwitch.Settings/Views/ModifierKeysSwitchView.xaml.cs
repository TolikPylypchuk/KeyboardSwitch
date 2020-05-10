using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Converters;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class ModifierKeysSwitchView : ReactiveUserControl<ModifierKeysSwitchViewModel>
    {
        private readonly ModifierKeyConverter modifierKeyConverter = new ModifierKeyConverter();
        private readonly NumberConverter numberConverter = new NumberConverter();

        public ModifierKeysSwitchView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.Bind(
                    this.ViewModel,
                    vm => vm.ForwardModifierKeys,
                    v => v.ForwardComboBox.SelectedItem,
                    null,
                    this.modifierKeyConverter,
                    this.modifierKeyConverter)
                    .DisposeWith(disposables);

                this.Bind(
                        this.ViewModel,
                        vm => vm.BackwardModifierKeys,
                        v => v.BackwardComboBox.SelectedItem,
                        null,
                        this.modifierKeyConverter,
                        this.modifierKeyConverter)
                    .DisposeWith(disposables);

                this.Bind(
                        this.ViewModel,
                        vm => vm.PressCount,
                        v => v.PressCountTextBox.Text,
                        null,
                        this.numberConverter,
                        this.numberConverter)
                    .DisposeWith(disposables);

                this.Bind(
                        this.ViewModel,
                        vm => vm.WaitMilliseconds,
                        v => v.WaitMillisecondsTextBox.Text,
                        null,
                        this.numberConverter,
                        this.numberConverter)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ComboBox ForwardComboBox { get; set; } = null!;
        private ComboBox BackwardComboBox { get; set; } = null!;

        private TextBox PressCountTextBox { get; set; } = null!;
        private TextBox WaitMillisecondsTextBox { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.ForwardComboBox = this.FindControl<ComboBox>(nameof(this.ForwardComboBox));
            this.BackwardComboBox = this.FindControl<ComboBox>(nameof(this.BackwardComboBox));

            this.PressCountTextBox = this.FindControl<TextBox>(nameof(this.PressCountTextBox));
            this.WaitMillisecondsTextBox = this.FindControl<TextBox>(nameof(this.WaitMillisecondsTextBox));

            var allModifiers = new List<ModifierKeys> { ModifierKeys.Alt, ModifierKeys.Ctrl, ModifierKeys.Shift }
                .GetPowerSet()
                .Select(modifiers => modifiers.ToList())
                .OrderBy(modifiers => modifiers.Count)
                .Skip(1)
                .Select(modifiers => modifiers.Flatten())
                .Select(modifiers => this.modifierKeyConverter.TryConvert(modifiers, typeof(string), null, out var str)
                    ? str?.ToString()
                    : null)
                .Where(value => value != null)
                .ToList();

            this.ForwardComboBox.Items = allModifiers;
            this.BackwardComboBox.Items = allModifiers;
        }
    }
}

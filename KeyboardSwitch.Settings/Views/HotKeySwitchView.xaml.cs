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
using ReactiveUI.Validation.Extensions;

namespace KeyboardSwitch.Settings.Views
{
    public class HotKeySwitchView : ReactiveUserControl<HotKeySwitchViewModel>
    {
        private readonly ModifierKeyConverter modifierKeyConverter = new ModifierKeyConverter();

        public HotKeySwitchView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.Bind(
                        this.ViewModel,
                        vm => vm.ModifierKeys,
                        v => v.ModifierKeysComboBox.SelectedItem,
                        null,
                        this.modifierKeyConverter,
                        this.modifierKeyConverter)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Forward, v => v.ForwardContent.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Backward, v => v.BackwardContent.Content)
                    .DisposeWith(disposables);

                this.BindValidation(
                        this.ViewModel, vm => vm.ForwardIsValidRule, v => v.ForwardValidationTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                        this.ViewModel, vm => vm.ForwardIsRequiredRule, v => v.ForwardValidationRequiredTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                        this.ViewModel, vm => vm.BackwardIsValidRule, v => v.BackwardValidationTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                        this.ViewModel,
                        vm => vm.BackwardIsRequiredRule,
                        v => v.BackwardValidationRequiredTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                        this.ViewModel,
                        vm => vm.SwitchMethodsAreDifferentRule,
                        v => v.SwitchMethodsValidationTextBlock.Text)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ComboBox ModifierKeysComboBox { get; set; } = null!;

        private ContentControl ForwardContent { get; set; } = null!;
        private ContentControl BackwardContent { get; set; } = null!;

        private TextBlock ForwardValidationRequiredTextBlock { get; set; } = null!;
        private TextBlock ForwardValidationTextBlock { get; set; } = null!;

        private TextBlock BackwardValidationRequiredTextBlock { get; set; } = null!;
        private TextBlock BackwardValidationTextBlock { get; set; } = null!;

        private TextBlock SwitchMethodsValidationTextBlock { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.ModifierKeysComboBox = this.FindControl<ComboBox>(nameof(this.ModifierKeysComboBox));

            this.ForwardContent = this.FindControl<ContentControl>(nameof(this.ForwardContent));
            this.BackwardContent = this.FindControl<ContentControl>(nameof(this.BackwardContent));

            this.ForwardValidationRequiredTextBlock = this.FindControl<TextBlock>(
                nameof(this.ForwardValidationRequiredTextBlock));

            this.ForwardValidationTextBlock = this.FindControl<TextBlock>(
                nameof(this.ForwardValidationTextBlock));

            this.BackwardValidationRequiredTextBlock = this.FindControl<TextBlock>(
                nameof(this.BackwardValidationRequiredTextBlock));

            this.BackwardValidationTextBlock = this.FindControl<TextBlock>(
                nameof(this.BackwardValidationTextBlock));

            this.SwitchMethodsValidationTextBlock = this.FindControl<TextBlock>(
                nameof(this.SwitchMethodsValidationTextBlock));

            this.ModifierKeysComboBox.Items = new List<ModifierKeys>
                {
                    ModifierKeys.Alt, ModifierKeys.Ctrl, ModifierKeys.Shift
                }
                .GetPowerSet()
                .Select(modifiers => modifiers.ToList())
                .Where(modifiers => modifiers.Count > 1)
                .OrderBy(modifiers => modifiers.Count)
                .Select(modifiers => modifiers.Flatten())
                .Select(modifiers => this.modifierKeyConverter.TryConvert(modifiers, typeof(string), null, out var str)
                    ? str?.ToString()
                    : null)
                .Where(value => value != null)
                .ToList();
        }
    }
}

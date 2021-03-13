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
    public partial class HotKeySwitchView : ReactiveUserControl<HotKeySwitchViewModel>
    {
        public HotKeySwitchView()
        {
            this.InitializeComponent();
            this.ModifierKeysComboBox.Items = new List<ModifierKeys>
                {
                    ModifierKeys.Alt, ModifierKeys.Ctrl, ModifierKeys.Shift
                }
                .GetPowerSet()
                .Select(modifiers => modifiers.ToList())
                .Where(modifiers => modifiers.Count > 1)
                .OrderBy(modifiers => modifiers.Count)
                .Select(modifiers => modifiers.Flatten())
                .Select(Convert.ModifierKeysToString)
                .Where(value => value != null)
                .ToList();

            this.WhenActivated(disposables =>
            {
                this.Bind(this.ViewModel, vm => vm.ModifierKeys, v => v.ModifierKeysComboBox.SelectedItem)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Forward, v => v.ForwardContent.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Backward, v => v.BackwardContent.Content)
                    .DisposeWith(disposables);

                this.BindValidation(
                    this.ViewModel, vm => vm!.ForwardIsValidRule, v => v.ForwardValidationTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                    this.ViewModel, vm => vm!.ForwardIsRequiredRule, v => v.ForwardValidationRequiredTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                    this.ViewModel, vm => vm!.BackwardIsValidRule, v => v.BackwardValidationTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindValidation(
                    this.ViewModel,
                    vm => vm!.BackwardIsRequiredRule,
                    v => v.BackwardValidationRequiredTextBlock.Text)
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Core;
using KeyboardSwitch.Core.Keyboard;
using KeyboardSwitch.Settings.Controls;
using KeyboardSwitch.Settings.Converters;
using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Properties;

using ReactiveUI;
using ReactiveUI.Validation.Extensions;

using static KeyboardSwitch.Core.Util;

using Convert = KeyboardSwitch.Settings.Converters.Convert;

namespace KeyboardSwitch.Settings.Views
{
    public partial class PreferencesView : ReactiveUserControl<PreferencesViewModel>
    {
        private readonly KeyCodeConverter keyCodeConverter = new();

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

                if (this.ViewModel!.SwitchLayoutsViaKeyboardSimulation)
                {
                    this.BindLayoutKeys(disposables);
                } else
                {
                    this.LayoutKeysPanel.IsVisible = false;
                }
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

            this.Bind(this.ViewModel, vm => vm.ShowConverter, v => v.ShowConverterCheckBox.IsChecked)
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

            this.Bind(this.ViewModel, vm => vm.PressCount, v => v.PressCountBox.Value)
                .DisposeWith(disposables);

            this.Bind(this.ViewModel, vm => vm.WaitMilliseconds, v => v.WaitMillisecondsBox.Value)
                .DisposeWith(disposables);
        }

        private void BindLayoutKeys(CompositeDisposable disposables)
        {
            this.ViewModel!.LayoutForwardKeyCodes
                .ToObservableChangeSet()
                .ToCollection()
                .Select(this.ConvertKeyCodesToString)
                .BindTo(this, v => v.LayoutForwardKeysTextBox.Text)
                .DisposeWith(disposables);

            this.ViewModel!.LayoutBackwardKeyCodes
                .ToObservableChangeSet()
                .ToCollection()
                .Select(this.ConvertKeyCodesToString)
                .BindTo(this, v => v.LayoutBackwardKeysTextBox.Text)
                .DisposeWith(disposables);

            this.LayoutForwardKeysTextBox.GetObservable(KeysBox.KeyPressedEvent)
                .Select(e => e.Key)
                .Select(this.keyCodeConverter.ConvertToKeyCode)
                .WhereValueNotNull()
                .InvokeCommand(this.ViewModel!.AddLayoutForwardKeyCode)
                .DisposeWith(disposables);

            this.LayoutBackwardKeysTextBox.GetObservable(KeysBox.KeyPressedEvent)
                .Select(e => e.Key)
                .Select(this.keyCodeConverter.ConvertToKeyCode)
                .WhereValueNotNull()
                .InvokeCommand(this.ViewModel!.AddLayoutBackwardKeyCode)
                .DisposeWith(disposables);

            this.BindCommand(
                this.ViewModel, vm => vm.ClearLayoutForwardKeyCodes, v => v.ClearLayoutForwardKeysButton)
                .DisposeWith(disposables);

            this.BindCommand(
                this.ViewModel, vm => vm.ClearLayoutBackwardKeyCodes, v => v.ClearLayoutBackwardKeysButton)
                .DisposeWith(disposables);

            var shouldShowManualMetaButtons = PlatformDependent(
                windows: () => false, macos: () => false, linux: () => true);

            if (shouldShowManualMetaButtons)
            {
                this.EnableManualMetaButtons(disposables);
            } else
            {
                this.AddLeftMetaForwardButton.IsVisible = false;
                this.AddRightMetaForwardButton.IsVisible = false;
                this.AddLeftMetaBackwardButton.IsVisible = false;
                this.AddRightMetaBackwardButton.IsVisible = false;
            } 
        }

        private void EnableManualMetaButtons(CompositeDisposable disposables)
        {
            this.AddLeftMetaForwardButton.Content =
                    String.Format(Messages.AddKeyFormat, Messages.ModifierKeyLeftSuper);

            this.AddRightMetaForwardButton.Content =
                String.Format(Messages.AddKeyFormat, Messages.ModifierKeyRightSuper);

            this.AddLeftMetaBackwardButton.Content =
                String.Format(Messages.AddKeyFormat, Messages.ModifierKeyLeftSuper);

            this.AddRightMetaBackwardButton.Content =
                String.Format(Messages.AddKeyFormat, Messages.ModifierKeyRightSuper);

            this.AddLeftMetaForwardButton.GetObservable(Button.ClickEvent)
                .Select(e => KeyCode.VcLeftMeta)
                .InvokeCommand(this.ViewModel!.AddLayoutForwardKeyCode)
                .DisposeWith(disposables);

            this.AddRightMetaForwardButton.GetObservable(Button.ClickEvent)
                .Select(e => KeyCode.VcRightMeta)
                .InvokeCommand(this.ViewModel!.AddLayoutForwardKeyCode)
                .DisposeWith(disposables);

            this.AddLeftMetaBackwardButton.GetObservable(Button.ClickEvent)
                .Select(e => KeyCode.VcLeftMeta)
                .InvokeCommand(this.ViewModel!.AddLayoutBackwardKeyCode)
                .DisposeWith(disposables);

            this.AddRightMetaBackwardButton.GetObservable(Button.ClickEvent)
                .Select(e => KeyCode.VcRightMeta)
                .InvokeCommand(this.ViewModel!.AddLayoutBackwardKeyCode)
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

            this.BindValidation(
                this.ViewModel,
                vm => vm!.LayoutForwardKeysAreNotEmptyRule,
                v => v.LayoutForwardKeysAreNotEmptyValidationTextBlock.Text)
                .DisposeWith(disposables);

            this.BindValidation(
                this.ViewModel,
                vm => vm!.LayoutBackwardKeysAreNotEmptyRule,
                v => v.LayoutBackwardKeysAreNotEmptyValidationTextBlock.Text)
                .DisposeWith(disposables);

            this.BindValidation(
                this.ViewModel,
                vm => vm!.LayoutKeysAreDifferentRule,
                v => v.LayoutKeysAreDifferentValidationTextBlock.Text)
                .DisposeWith(disposables);
        }

        private void BindCommands(CompositeDisposable disposables)
        {
            this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                .DisposeWith(disposables);

            this.ViewModel!.Cancel.CanExecute
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

        private string ConvertKeyCodesToString(IEnumerable<KeyCode> keyCodes)
        {
            string names = keyCodes
                .Select(this.keyCodeConverter.GetName)
                .Aggregate(String.Empty, (acc, KeyCode) => $"{acc}+{KeyCode}");

            return !String.IsNullOrEmpty(names) ? names[1..] : names;
        }
    }
}

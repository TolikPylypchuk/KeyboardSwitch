using Convert = KeyboardSwitch.Settings.Converters.Convert;

namespace KeyboardSwitch.Settings.Views;

public partial class PreferencesView : ReactiveUserControl<PreferencesViewModel>
{
    public PreferencesView()
    {
        this.InitializeComponent();

        var modifiers = this.Modifiers()
            .Where(key => key != ModifierMask.None)
            .Select(Convert.ModifierToString)
            .ToImmutableList();

        var allModifiers = modifiers.Insert(0, Convert.ModifierToString(ModifierMask.None));

        this.ForwardFirstComboBox.ItemsSource = modifiers;
        this.ForwardSecondComboBox.ItemsSource = allModifiers;
        this.ForwardThirdComboBox.ItemsSource = allModifiers;

        this.BackwardFirstComboBox.ItemsSource = modifiers;
        this.BackwardSecondComboBox.ItemsSource = allModifiers;
        this.BackwardThirdComboBox.ItemsSource = allModifiers;

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
        this.Bind(this.ViewModel, vm => vm.ForwardModifierFirst, v => v.ForwardFirstComboBox.SelectedItem)
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.ForwardModifierSecond, v => v.ForwardSecondComboBox.SelectedItem)
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.ForwardModifierThird, v => v.ForwardThirdComboBox.SelectedItem)
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.BackwardModifierFirst, v => v.BackwardFirstComboBox.SelectedItem)
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.BackwardModifierSecond, v => v.BackwardSecondComboBox.SelectedItem)
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.BackwardModifierThird, v => v.BackwardThirdComboBox.SelectedItem)
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.PressCount, v => v.PressCountBox.Value)
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.WaitMilliseconds, v => v.WaitMillisecondsBox.Value)
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

        this.ViewModel!.Cancel.CanExecute
            .BindTo(this, v => v.ActionPanel.IsVisible)
            .DisposeWith(disposables);
    }

    private List<ModifierMask> Modifiers() =>
        [
            ModifierMask.None,
            ModifierMask.Ctrl,
            ModifierMask.Shift,
            ModifierMask.Alt,
            ModifierMask.Meta,
            ModifierMask.LeftCtrl,
            ModifierMask.LeftShift,
            ModifierMask.LeftAlt,
            ModifierMask.LeftMeta,
            ModifierMask.RightCtrl,
            ModifierMask.RightShift,
            ModifierMask.RightAlt,
            ModifierMask.RightMeta
        ];
}

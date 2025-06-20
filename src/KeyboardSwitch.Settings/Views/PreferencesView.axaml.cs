using Convert = KeyboardSwitch.Settings.Converters.Convert;

namespace KeyboardSwitch.Settings.Views;

public partial class PreferencesView : ReactiveUserControl<PreferencesViewModel>
{
    public PreferencesView()
    {
        this.InitializeComponent();

        var modifiers = this.Modifiers()
            .Where(key => key != EventMask.None)
            .Select(Convert.ModifierToString)
            .ToImmutableList();

        var allModifiers = modifiers.Insert(0, Convert.ModifierToString(EventMask.None));

        this.ForwardFirstComboBox.ItemsSource = modifiers;
        this.ForwardSecondComboBox.ItemsSource = allModifiers;
        this.ForwardThirdComboBox.ItemsSource = allModifiers;

        this.BackwardFirstComboBox.ItemsSource = modifiers;
        this.BackwardSecondComboBox.ItemsSource = allModifiers;
        this.BackwardThirdComboBox.ItemsSource = allModifiers;

        this.AppThemeComboBox.ItemsSource = Enum.GetValues<AppTheme>()
            .Select(Convert.AppThemeToString)
            .ToImmutableList();

        this.AppThemeVariantComboBox.ItemsSource = Enum.GetValues<AppThemeVariant>()
            .Select(Convert.AppThemeVariantToString)
            .ToImmutableList();

        this.WhenActivated(disposables =>
        {
            this.BindCheckboxes(disposables);
            this.BindControls(disposables);
            this.BindValidations(disposables);
            this.BindCommands(disposables);

            if (this.ViewModel?.ShowUseXsel == true)
            {
                this.UseXselCheckBox.IsVisible = true;
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

        this.Bind(this.ViewModel, vm => vm.UseXsel, v => v.UseXselCheckBox.IsChecked)
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

        this.Bind(this.ViewModel, vm => vm.PressCount, v => v.PressCountBox.Value, v => v, v => (int)(v ?? 0M))
            .DisposeWith(disposables);

        this.Bind(
            this.ViewModel, vm => vm.WaitMilliseconds, v => v.WaitMillisecondsBox.Value, v => v, v => (int)(v ?? 0M))
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.AppTheme, v => v.AppThemeComboBox.SelectedItem)
            .DisposeWith(disposables);

        this.Bind(this.ViewModel, vm => vm.AppThemeVariant, v => v.AppThemeVariantComboBox.SelectedItem)
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

        this.PressCountValidationTextBlock.WhenAnyValue(v => v.Text)
            .Select(text => text is { Length: > 0 })
            .BindTo(this, v => v.PressCountValidationTextBlock.IsVisible);

        this.WaitMillisecondsValidationTextBlock.WhenAnyValue(v => v.Text)
            .Select(text => text is { Length: > 0 })
            .BindTo(this, v => v.WaitMillisecondsValidationTextBlock.IsVisible);

        this.ModifierKeysValidationTextBlock.WhenAnyValue(v => v.Text)
            .Select(text => text is { Length: > 0 })
            .BindTo(this, v => v.ModifierKeysValidationTextBlock.IsVisible);

        this.SwitchMethodsValidationTextBlock.WhenAnyValue(v => v.Text)
            .Select(text => text is { Length: > 0 })
            .BindTo(this, v => v.SwitchMethodsValidationTextBlock.IsVisible);
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

    private List<EventMask> Modifiers() =>
        [
            EventMask.None,
            EventMask.Ctrl,
            EventMask.Shift,
            EventMask.Alt,
            EventMask.Meta,
            EventMask.LeftCtrl,
            EventMask.LeftShift,
            EventMask.LeftAlt,
            EventMask.LeftMeta,
            EventMask.RightCtrl,
            EventMask.RightShift,
            EventMask.RightAlt,
            EventMask.RightMeta
        ];
}

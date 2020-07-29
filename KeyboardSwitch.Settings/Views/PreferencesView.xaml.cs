using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Properties;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class PreferencesView : ReactiveUserControl<PreferencesViewModel>
    {
        public PreferencesView()
        {
            this.WhenActivated(disposables =>
            {
                this.Bind(this.ViewModel, vm => vm.SwitchMode, v => v.SwitchModeComboBox.SelectedItem)
                    ?.DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.InstantSwitching, v => v.InstantSwitchingCheckBox.IsChecked)
                    ?.DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.SwitchLayout, v => v.SwitchLayoutCheckBox.IsChecked)
                    ?.DisposeWith(disposables);

                this.Bind(this.ViewModel, vm => vm.Startup, v => v.StartupCheckBox.IsChecked)
                    ?.DisposeWith(disposables);

                this.Bind(
                    this.ViewModel,
                    vm => vm.ShowUninstalledLayoutsMessage,
                    v => v.ShowRemovedLayoutsMessageCheckBox.IsChecked)
                    ?.DisposeWith(disposables);

                Observable.CombineLatest(
                        this.WhenAnyObservable(v => v.ViewModel.HotKeySwitchViewModel.Valid),
                        this.WhenAnyObservable(v => v.ViewModel.ModifierKeysSwitchModel.Valid))
                    .AllTrue()
                    .BindTo(this, v => v.SwitchModeComboBox.IsEnabled)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Content, v => v.PreferencesContent.Content)
                    ?.DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                    .DisposeWith(disposables);

                this.ViewModel.Cancel.CanExecute
                    .BindTo(this, v => v.ActionPanel.IsVisible)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private CheckBox InstantSwitchingCheckBox { get; set; } = null!;
        private CheckBox SwitchLayoutCheckBox { get; set; } = null!;
        private CheckBox StartupCheckBox { get; set; } = null!;
        private CheckBox ShowRemovedLayoutsMessageCheckBox { get; set; } = null!;

        private ComboBox SwitchModeComboBox { get; set; } = null!;
        private ContentControl PreferencesContent { get; set; } = null!;

        private StackPanel ActionPanel { get; set; } = null!;
        private Button SaveButton { get; set; } = null!;
        private Button CancelButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.InstantSwitchingCheckBox = this.FindControl<CheckBox>(nameof(this.InstantSwitchingCheckBox));
            this.SwitchLayoutCheckBox = this.FindControl<CheckBox>(nameof(this.SwitchLayoutCheckBox));
            this.StartupCheckBox = this.FindControl<CheckBox>(nameof(this.StartupCheckBox));
            this.ShowRemovedLayoutsMessageCheckBox = this.FindControl<CheckBox>(
                nameof(this.ShowRemovedLayoutsMessageCheckBox));

            this.SwitchModeComboBox = this.FindControl<ComboBox>(nameof(this.SwitchModeComboBox));
            this.PreferencesContent = this.FindControl<ContentControl>(nameof(this.PreferencesContent));

            this.ActionPanel = this.FindControl<StackPanel>(nameof(this.ActionPanel));
            this.SaveButton = this.FindControl<Button>(nameof(this.SaveButton));
            this.CancelButton = this.FindControl<Button>(nameof(this.CancelButton));

            this.SwitchModeComboBox.Items = new List<string> { Messages.ModifierKeys, Messages.HotKey };
        }
    }
}

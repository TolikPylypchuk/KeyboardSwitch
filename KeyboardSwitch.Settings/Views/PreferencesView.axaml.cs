using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Core.ViewModels;
using KeyboardSwitch.Settings.Properties;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public partial class PreferencesView : ReactiveUserControl<PreferencesViewModel>
    {
        public PreferencesView()
        {
            this.InitializeComponent();
            this.SwitchModeComboBox.Items = new List<string> { Messages.ModifierKeys, Messages.HotKey };
            this.WhenActivated(disposables =>
            {
                this.Bind(this.ViewModel, vm => vm.SwitchMode, v => v.SwitchModeComboBox.SelectedItem)
                    .DisposeWith(disposables);

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

                Observable.CombineLatest(
                    this.WhenAnyObservable(v => v.ViewModel.HotKeySwitchViewModel.Valid),
                    this.WhenAnyObservable(v => v.ViewModel.ModifierKeysSwitchModel.Valid))
                    .AllTrue()
                    .BindTo(this, v => v.SwitchModeComboBox.IsEnabled)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Content, v => v.PreferencesContent.Content)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                    .DisposeWith(disposables);

                this.ViewModel.Cancel.CanExecute
                    .BindTo(this, v => v.ActionPanel.IsVisible)
                    .DisposeWith(disposables);
            });
        }
    }
}

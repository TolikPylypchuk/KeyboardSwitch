using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Converters;
using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class PreferencesView : ReactiveUserControl<PreferencesViewModel>
    {
        private readonly SwitchModeConverter switchModeConverter = new SwitchModeConverter();

        public PreferencesView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.Bind(
                        this.ViewModel,
                        vm => vm.SwitchMode,
                        v => v.SwitchModeComboBox.SelectedItem,
                        null,
                        this.switchModeConverter,
                        this.switchModeConverter)
                    .DisposeWith(disposables);

                Observable.CombineLatest(
                        this.WhenAnyObservable(v => v.ViewModel.HotKeySwitchViewModel.FormChanged),
                        this.WhenAnyObservable(v => v.ViewModel.ModifierKeysSwitchModel.FormChanged))
                    .AnyTrue()
                    .Invert()
                    .BindTo(this, v => v.SwitchModeComboBox.IsEnabled)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Content, v => v.PreferencesContent.Content)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Save, v => v.SaveButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.Cancel, v => v.CancelButton)
                    .DisposeWith(disposables);

                this.WhenAnyObservable(v => v.ViewModel.Cancel.CanExecute)
                    .BindTo(this, v => v.ActionPanel.IsVisible)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ComboBox SwitchModeComboBox { get; set; } = null!;
        private ContentControl PreferencesContent { get; set; } = null!;

        private StackPanel ActionPanel { get; set; } = null!;
        private Button SaveButton { get; set; } = null!;
        private Button CancelButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.SwitchModeComboBox = this.FindControl<ComboBox>(nameof(this.SwitchModeComboBox));
            this.PreferencesContent = this.FindControl<ContentControl>(nameof(this.PreferencesContent));

            this.ActionPanel = this.FindControl<StackPanel>(nameof(this.ActionPanel));
            this.SaveButton = this.FindControl<Button>(nameof(this.SaveButton));
            this.CancelButton = this.FindControl<Button>(nameof(this.CancelButton));

            this.SwitchModeComboBox.Items = Enum.GetValues(typeof(SwitchMode))
                .Cast<SwitchMode>()
                .Select(mode => this.switchModeConverter.TryConvert(mode, typeof(string), null, out var result)
                    ? result?.ToString()
                    : null)
                .Where(value => value != null)
                .ToList();
        }
    }
}

using System;
using System.Linq;
using System.Reactive.Disposables;

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

                this.WhenAnyObservable(v => v.ViewModel.FormChanged)
                    .Invert()
                    .BindTo(this, v => v.SwitchModeComboBox.IsEnabled)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.Content, v => v.PreferencesContent.Content)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private ComboBox SwitchModeComboBox { get; set; } = null!;
        private ContentControl PreferencesContent { get; set; } = null!;


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.SwitchModeComboBox = this.FindControl<ComboBox>(nameof(this.SwitchModeComboBox));
            this.PreferencesContent = this.FindControl<ContentControl>(nameof(this.PreferencesContent));

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

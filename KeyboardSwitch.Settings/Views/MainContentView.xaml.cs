using System;
using System.Reactive.Disposables;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class MainContentView : ReactiveUserControl<MainContentViewModel>
    {
        public MainContentView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this.ViewModel, vm => vm.CharMappingViewModel, v => v.CharMappingTabItem.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.PreferencesViewModel, v => v.PreferencesTabItem.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.ConverterViewModel, v => v.ConverterTabItem.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(
                    this.ViewModel, vm => vm.ConverterSettingsViewModel, v => v.ConverterSettingsTabItem.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.AboutViewModel, v => v.AboutTabItem.Content)
                    .DisposeWith(disposables);

                this.ViewModel.OpenAboutTab
                    .Subscribe(_ => this.AboutTabItem.IsSelected = true)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TabItem CharMappingTabItem { get; set; } = null!;
        private TabItem PreferencesTabItem { get; set; } = null!;
        private TabItem ConverterTabItem { get; set; } = null!;
        private TabItem ConverterSettingsTabItem { get; set; } = null!;
        private TabItem AboutTabItem { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.CharMappingTabItem = this.FindControl<TabItem>(nameof(this.CharMappingTabItem));
            this.PreferencesTabItem = this.FindControl<TabItem>(nameof(this.PreferencesTabItem));
            this.ConverterTabItem = this.FindControl<TabItem>(nameof(this.ConverterTabItem));
            this.ConverterSettingsTabItem = this.FindControl<TabItem>(nameof(this.ConverterSettingsTabItem));
            this.AboutTabItem = this.FindControl<TabItem>(nameof(this.AboutTabItem));
        }
    }
}

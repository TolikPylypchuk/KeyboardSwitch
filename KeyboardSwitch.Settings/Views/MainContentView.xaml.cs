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
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.SwitchSettings, v => v.SwitchSettingsTabItem.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.OtherSettings, v => v.OtherSettingsTabItem.Content)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TabItem SwitchSettingsTabItem { get; set; } = null!;
        private TabItem OtherSettingsTabItem { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.SwitchSettingsTabItem = this.FindControl<TabItem>(nameof(this.SwitchSettingsTabItem));
            this.OtherSettingsTabItem = this.FindControl<TabItem>(nameof(this.OtherSettingsTabItem));
        }
    }
}

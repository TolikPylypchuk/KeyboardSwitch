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

                this.OneWayBind(this.ViewModel, vm => vm.SwitchSettings, v => v.CharMappingTabItem.Content)
                    .DisposeWith(disposables);

                this.OneWayBind(this.ViewModel, vm => vm.OtherSettings, v => v.PreferencesTabItem.Content)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TabItem CharMappingTabItem { get; set; } = null!;
        private TabItem PreferencesTabItem { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.CharMappingTabItem = this.FindControl<TabItem>(nameof(this.CharMappingTabItem));
            this.PreferencesTabItem = this.FindControl<TabItem>(nameof(this.PreferencesTabItem));
        }
    }
}

namespace KeyboardSwitch.Settings.Views;

public partial class MainContentView : ReactiveUserControl<MainContentViewModel>
{
    public MainContentView()
    {
        this.InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(this.ViewModel, vm => vm.CharMappingViewModel, v => v.CharMappingTabItem.Content)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.PreferencesViewModel, v => v.PreferencesTabItem.Content)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.ConverterViewModel, v => v.ConverterTabItem.Content)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.ShowConverter, v => v.ConverterTabItem.IsVisible)
                .DisposeWith(disposables);

            this.OneWayBind(
                this.ViewModel, vm => vm.ConverterSettingsViewModel, v => v.ConverterSettingsTabItem.Content)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.ShowConverter, v => v.ConverterSettingsTabItem.IsVisible)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.AboutViewModel, v => v.AboutTabItem.Content)
                .DisposeWith(disposables);

            this.ViewModel!.OpenAboutTab
                .Subscribe(_ => this.AboutTabItem.IsSelected = true)
                .DisposeWith(disposables);
        });
    }
}

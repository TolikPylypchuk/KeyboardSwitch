namespace KeyboardSwitch.Settings.Views;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        this.InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(this.ViewModel, vm => vm.MainContentViewModel, v => v.MainContent.Content)
                .DisposeWith(disposables);

            this.OneWayBind(this.ViewModel, vm => vm.ServiceViewModel, v => v.ServiceViewContent.Content)
                .DisposeWith(disposables);

            var layoutServiceStatus = this.ViewModel!.LayoutServiceStatus;

            this.LayoutServiceWarningBorder.IsVisible = layoutServiceStatus != LayoutServiceStatus.Ok;
            this.CannotSwitchLayoutsTextBlock.IsVisible =
                layoutServiceStatus == LayoutServiceStatus.CannotSwitchLayouts;
            this.LayoutsUnavailableTextBlock.IsVisible = layoutServiceStatus == LayoutServiceStatus.Unavailable;

            this.ViewModel.OpenExternallyCommand
                .Subscribe(this.BringToForeground)
                .DisposeWith(disposables);

            this.GetObservable(KeyUpEvent)
                .Select(e => e.Key)
                .Where(key => key == Key.F1)
                .Discard()
                .InvokeCommand(this.ViewModel.OpenAboutTabCommand)
                .DisposeWith(disposables);
        });
    }

    private void BringToForeground()
    {
        if (!this.IsVisible)
        {
            this.Show();
        }

        if (this.WindowState == WindowState.Minimized)
        {
            this.WindowState = WindowState.Normal;
        }

        this.Activate();
        this.Topmost = true;
        this.Topmost = false;
        this.Focus();
    }
}

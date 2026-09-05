namespace KeyboardSwitch.Settings.Views;

public partial class ServiceView : ReactiveUserControl<ServiceViewModel>
{
    public ServiceView()
    {
        this.InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(v => v.ViewModel!.ServiceStatus)
                .Select(status => status == ServiceStatus.Running)
                .BindTo(this, v => v.ServiceRunningTextBlock.IsVisible)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel!.ServiceStatus)
                .Select(status => status == ServiceStatus.Stopped)
                .BindTo(this, v => v.ServiceNotRunningTextBlock.IsVisible)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel!.ServiceStatus)
                .Select(status => status == ServiceStatus.ShuttingDown)
                .BindTo(this, v => v.ServiceShuttingDownTextBlock.IsVisible)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel!.ServiceStatus)
                .Select(status => status == ServiceStatus.Running)
                .BindTo(this, v => v.StopServiceButton.IsVisible)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel!.ServiceStatus)
                .Select(status => status == ServiceStatus.Stopped)
                .BindTo(this, v => v.StartServiceButton.IsVisible)
                .DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel!.ServiceStatus)
                .Select(status => status == ServiceStatus.ShuttingDown)
                .BindTo(this, v => v.KillServiceButton.IsVisible)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.StartServiceCommand, v => v.StartServiceButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.StopServiceCommand, v => v.StopServiceButton)
                .DisposeWith(disposables);

            this.BindCommand(this.ViewModel, vm => vm.KillServiceCommand, v => v.KillServiceButton)
                .DisposeWith(disposables);
        });
    }
}

namespace KeyboardSwitch.Settings.Core.ViewModels;

public enum ServiceStatus { Running, Stopped, ShuttingDown }

public sealed partial class ServiceViewModel : ReactiveObject
{
    private readonly IServiceCommunicator serviceCommunicator;

    [ObservableAsProperty]
    private ServiceStatus serviceStatus;

    private bool isShutdownRequested = false;

    public ServiceViewModel(IServiceCommunicator? serviceCommunicator = null, IScheduler? scheduler = null)
    {
        this.serviceCommunicator = serviceCommunicator ?? AppLocator.Current.GetRequiredService<IServiceCommunicator>();

        scheduler ??= RxSchedulers.MainThreadScheduler;

        var serviceStatus = new Subject<ServiceStatus>();

        this.serviceStatusHelper = serviceStatus.ToProperty(this, vm => vm.ServiceStatus);

        var canStartService = serviceStatus.Select(status => status == ServiceStatus.Stopped);
        var canStopService = serviceStatus.Select(status => status == ServiceStatus.Running);
        var canKillService = serviceStatus.Select(status => status == ServiceStatus.ShuttingDown);

        Observable.Interval(TimeSpan.FromSeconds(1), scheduler)
            .Select(_ => this.CheckServiceStatus())
            .Merge(this.StartServiceCommand.Select(_ => ServiceStatus.Running))
            .Merge(this.StopServiceCommand.Select(_ => ServiceStatus.ShuttingDown))
            .Merge(this.KillServiceCommand.Select(_ => ServiceStatus.Stopped))
            .DistinctUntilChanged()
            .Subscribe(serviceStatus);
    }

    private ServiceStatus CheckServiceStatus()
    {
        bool isRunning = this.serviceCommunicator.IsServiceRunning();

        if (!isRunning)
        {
            this.isShutdownRequested = false;
        }

        return isRunning
            ? isShutdownRequested ? ServiceStatus.ShuttingDown : ServiceStatus.Running
            : ServiceStatus.Stopped;
    }

    [ReactiveCommand]
    private void StartService() =>
        this.serviceCommunicator.StartService();

    [ReactiveCommand]
    private void StopService()
    {
        this.serviceCommunicator.StopService(kill: false);
        this.isShutdownRequested = true;
    }

    [ReactiveCommand]
    private void KillService() =>
        this.serviceCommunicator.StopService(kill: true);

    [ReactiveCommand]
    private void ReloadSettings()
    {
        if (this.ServiceStatus == ServiceStatus.Running)
        {
            this.serviceCommunicator.ReloadService();
        }
    }
}

namespace KeyboardSwitch.Settings.Core.ViewModels;

public enum ServiceStatus { Running, Stopped, ShuttingDown }

public sealed partial class ServiceViewModel : ReactiveObject
{
    private readonly IServiceCommunicator serviceCommunicator;

    private bool isShutdownRequested = false;

    public ServiceViewModel(IServiceCommunicator? serviceCommunicator = null, IScheduler? scheduler = null)
    {
        this.serviceCommunicator = serviceCommunicator ?? AppLocator.Current.GetRequiredService<IServiceCommunicator>();

        scheduler ??= RxSchedulers.MainThreadScheduler;

        var serviceStatus = new Subject<ServiceStatus>();

        this._serviceStatusHelper = serviceStatus.ToProperty(this, vm => vm.ServiceStatus);

        this.CanStartService = serviceStatus.Select(status => status == ServiceStatus.Stopped);
        this.CanStopService = serviceStatus.Select(status => status == ServiceStatus.Running);
        this.CanKillService = serviceStatus.Select(status => status == ServiceStatus.ShuttingDown);

        Observable.Interval(TimeSpan.FromSeconds(1), scheduler)
            .Select(_ => this.CheckServiceStatus())
            .Merge(this.StartServiceCommand.Select(_ => ServiceStatus.Running))
            .Merge(this.StopServiceCommand.Select(_ => ServiceStatus.ShuttingDown))
            .Merge(this.KillServiceCommand.Select(_ => ServiceStatus.Stopped))
            .DistinctUntilChanged()
            .Subscribe(serviceStatus);
    }

    [ObservableAsProperty]
    public partial ServiceStatus ServiceStatus { get; }

    private IObservable<bool> CanStartService { get; }
    private IObservable<bool> CanStopService { get; }
    private IObservable<bool> CanKillService { get; }

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

    [ReactiveCommand(CanExecute = nameof(CanStartService))]
    private void StartService() =>
        this.serviceCommunicator.StartService();

    [ReactiveCommand(CanExecute = nameof(CanStopService))]
    private void StopService()
    {
        this.serviceCommunicator.StopService(kill: false);
        this.isShutdownRequested = true;
    }

    [ReactiveCommand(CanExecute = nameof(CanKillService))]
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

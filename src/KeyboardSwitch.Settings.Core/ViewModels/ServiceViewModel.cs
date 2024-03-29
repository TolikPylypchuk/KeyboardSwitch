namespace KeyboardSwitch.Settings.Core.ViewModels;

public enum ServiceStatus { Running, Stopped, ShuttingDown }

public sealed class ServiceViewModel : ReactiveObject
{
    private readonly IServiceCommunicator serviceCommunicator;

    private readonly ObservableAsPropertyHelper<ServiceStatus> serviceStatus;

    private bool isShutdownRequested = false;

    public ServiceViewModel(IServiceCommunicator? serviceCommunicator = null, IScheduler? scheduler = null)
    {
        this.serviceCommunicator = serviceCommunicator ?? GetRequiredService<IServiceCommunicator>();

        scheduler ??= RxApp.MainThreadScheduler;

        var serviceStatus = new Subject<ServiceStatus>();

        this.serviceStatus = serviceStatus.ToProperty(this, vm => vm.ServiceStatus);

        var canStartService = serviceStatus.Select(status => status == ServiceStatus.Stopped);
        var canStopService = serviceStatus.Select(status => status == ServiceStatus.Running);
        var canKillService = serviceStatus.Select(status => status == ServiceStatus.ShuttingDown);

        this.StartService = ReactiveCommand.Create(this.OnStartService, canStartService);
        this.StopService = ReactiveCommand.Create(this.OnStopService, canStopService);
        this.KillService = ReactiveCommand.Create(this.OnKillService, canKillService);
        this.ReloadSettings = ReactiveCommand.Create(this.OnReloadSettings);

        Observable.Interval(TimeSpan.FromSeconds(1), scheduler)
            .Select(_ => this.CheckServiceStatus())
            .Merge(this.StartService.Select(_ => ServiceStatus.Running))
            .Merge(this.StopService.Select(_ => ServiceStatus.ShuttingDown))
            .Merge(this.KillService.Select(_ => ServiceStatus.Stopped))
            .DistinctUntilChanged()
            .Subscribe(serviceStatus);
    }

    public ServiceStatus ServiceStatus => this.serviceStatus.Value;

    public ReactiveCommand<Unit, Unit> StartService { get; }
    public ReactiveCommand<Unit, Unit> StopService { get; }
    public ReactiveCommand<Unit, Unit> KillService { get; }
    public ReactiveCommand<Unit, Unit> ReloadSettings { get; }

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

    private void OnStartService() =>
        this.serviceCommunicator.StartService();

    private void OnStopService()
    {
        this.serviceCommunicator.StopService(kill: false);
        this.isShutdownRequested = true;
    }

    private void OnKillService() =>
        this.serviceCommunicator.StopService(kill: true);

    private void OnReloadSettings()
    {
        if (this.ServiceStatus == ServiceStatus.Running)
        {
            this.serviceCommunicator.ReloadService();
        }
    }
}

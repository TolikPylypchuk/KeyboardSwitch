using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Services.Infrastructure;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public enum ServiceStatus { Running, Stopped, ShuttingDown }

    public sealed class ServiceViewModel : ReactiveObject
    {
        private readonly ISettingsService settingsService;
        private readonly INamedPipeService namedPipeService;

        private bool isShutdownRequested = false;

        public ServiceViewModel(
            ISettingsService? settingsService = null,
            INamedPipeService? namedPipeService = null,
            IScheduler? scheduler = null)
        {
            this.settingsService = settingsService ?? Locator.Current.GetService<ISettingsService>();
            this.namedPipeService = namedPipeService ??
                Locator.Current.GetService<ServiceResolver<INamedPipeService>>()(nameof(KeyboardSwitch));

            scheduler ??= RxApp.MainThreadScheduler;

            var serviceStatus = new Subject<ServiceStatus>();

            serviceStatus.ToPropertyEx(this, vm => vm.ServiceStatus);

            var canStartService = serviceStatus.Select(status => status == ServiceStatus.Stopped);
            var canStopService = serviceStatus.Select(status => status == ServiceStatus.Running);
            var canKillService = serviceStatus.Select(status => status == ServiceStatus.ShuttingDown);

            this.StartService = ReactiveCommand.CreateFromTask(this.StartServiceAsync, canStartService);
            this.StopService = ReactiveCommand.Create(this.OnStopService, canStopService);
            this.KillService = ReactiveCommand.Create(this.OnKillService, canKillService);

            Observable.Interval(TimeSpan.FromSeconds(1), scheduler)
                .Select(_ => this.CheckServiceStatus())
                .Merge(this.StartService.Select(_ => ServiceStatus.Running))
                .Merge(this.StopService.Select(_ => ServiceStatus.ShuttingDown))
                .Merge(this.KillService.Select(_ => ServiceStatus.Stopped))
                .DistinctUntilChanged()
                .Subscribe(serviceStatus);
        }

        public ServiceStatus ServiceStatus{ [ObservableAsProperty] get; }

        public ReactiveCommand<Unit, Unit> StartService { get; }
        public ReactiveCommand<Unit, Unit> StopService { get; }
        public ReactiveCommand<Unit, Unit> KillService { get; }

        private ServiceStatus CheckServiceStatus()
        {
            bool isRunning = Process.GetProcessesByName(nameof(KeyboardSwitch)).Length > 0;

            if (!isRunning)
            {
                this.isShutdownRequested = false;
            }

            return isRunning
                ? isShutdownRequested ? ServiceStatus.ShuttingDown : ServiceStatus.Running
                : ServiceStatus.Stopped;
        }

        private async Task StartServiceAsync()
        {
            var settings = await settingsService.GetSwitchSettingsAsync();
            Process.Start(settings.ServicePath);
        }

        private void OnStopService()
        {
            this.namedPipeService.Write(ExternalCommand.Stop);
            this.isShutdownRequested = true;
        }

        private void OnKillService()
            => Process.GetProcessesByName(nameof(KeyboardSwitch)).ForEach(process => process.Kill());
    }
}

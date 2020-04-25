using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using KeyboardSwitch.Common;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class ServiceViewModel : ReactiveObject
    {
        public ServiceViewModel(IScheduler? scheduler = null)
        {
            scheduler ??= RxApp.MainThreadScheduler;

            var isServiceRunning = new Subject<bool>();

            isServiceRunning.ToPropertyEx(this, vm => vm.IsServiceRunning);

            this.StartService = ReactiveCommand.Create(this.OnStartService, isServiceRunning.Invert());
            this.StopService = ReactiveCommand.Create(this.OnStopService, isServiceRunning);

            Observable.Interval(TimeSpan.FromSeconds(1), scheduler ?? RxApp.MainThreadScheduler)
                .Select(_ => this.CheckIfServiceIsRunning())
                .Merge(this.StartService.Select(_ => true))
                .Merge(this.StopService.Select(_ => false))
                .DistinctUntilChanged()
                .Subscribe(isServiceRunning);
        }

        public bool IsServiceRunning { [ObservableAsProperty] get; }

        public ReactiveCommand<Unit, Unit> StartService { get; }
        public ReactiveCommand<Unit, Unit> StopService { get; }

        private bool CheckIfServiceIsRunning()
            => Process.GetProcessesByName(nameof(KeyboardSwitch)).Length > 0;

        private void OnStartService()
            => Process.Start(nameof(KeyboardSwitch));

        private void OnStopService()
            => Process.GetProcessesByName(nameof(KeyboardSwitch)).ForEach(process => process.Kill());
    }
}

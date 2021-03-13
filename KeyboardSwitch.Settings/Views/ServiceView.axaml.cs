using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public partial class ServiceView : ReactiveUserControl<ServiceViewModel>
    {
        public ServiceView()
        {
            this.InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(v => v.ViewModel.ServiceStatus)
                    .Select(status => status == ServiceStatus.Running)
                    .BindTo(this, v => v.ServiceRunningTextBlock.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.ServiceStatus)
                    .Select(status => status == ServiceStatus.Stopped)
                    .BindTo(this, v => v.ServiceNotRunningTextBlock.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.ServiceStatus)
                    .Select(status => status == ServiceStatus.ShuttingDown)
                    .BindTo(this, v => v.ServiceShuttingDownTextBlock.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.ServiceStatus)
                    .Select(status => status == ServiceStatus.Running)
                    .BindTo(this, v => v.StopServiceButton.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.ServiceStatus)
                    .Select(status => status == ServiceStatus.Stopped)
                    .BindTo(this, v => v.StartServiceButton.IsVisible)
                    .DisposeWith(disposables);

                this.WhenAnyValue(v => v.ViewModel.ServiceStatus)
                    .Select(status => status == ServiceStatus.ShuttingDown)
                    .BindTo(this, v => v.KillServiceButton.IsVisible)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.StartService, v => v.StartServiceButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.StopService, v => v.StopServiceButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.KillService, v => v.KillServiceButton)
                    .DisposeWith(disposables);
            });
        }
    }
}

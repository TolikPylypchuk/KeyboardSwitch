using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

using KeyboardSwitch.Settings.Core.ViewModels;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Views
{
    public class ServiceView : ReactiveUserControl<ServiceViewModel>
    {
        public ServiceView()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(this, v => v.ViewModel, v => v.DataContext)
                    .DisposeWith(disposables);

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

            this.InitializeComponent();
        }

        private TextBlock ServiceRunningTextBlock { get; set; } = null!;
        private TextBlock ServiceNotRunningTextBlock { get; set; } = null!;
        private TextBlock ServiceShuttingDownTextBlock { get; set; } = null!;
        private Button StartServiceButton { get; set; } = null!;
        private Button StopServiceButton { get; set; } = null!;
        private Button KillServiceButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.ServiceRunningTextBlock = this.FindControl<TextBlock>(nameof(this.ServiceRunningTextBlock));
            this.ServiceNotRunningTextBlock = this.FindControl<TextBlock>(nameof(this.ServiceNotRunningTextBlock));
            this.ServiceShuttingDownTextBlock = this.FindControl<TextBlock>(nameof(this.ServiceShuttingDownTextBlock));
            this.StartServiceButton = this.FindControl<Button>(nameof(this.StartServiceButton));
            this.StopServiceButton = this.FindControl<Button>(nameof(this.StopServiceButton));
            this.KillServiceButton = this.FindControl<Button>(nameof(this.KillServiceButton));
        }
    }
}

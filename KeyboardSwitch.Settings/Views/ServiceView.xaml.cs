using System.Reactive.Disposables;

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

                this.OneWayBind(
                    this.ViewModel,
                    vm => vm.IsServiceRunning,
                    v => v.ServiceRunningTextBlock.IsVisible)
                    .DisposeWith(disposables);

                this.OneWayBind(
                    this.ViewModel,
                    vm => vm.IsServiceRunning,
                    v => v.ServiceNotRunningTextBlock.IsVisible,
                    value => !value)
                    .DisposeWith(disposables);

                this.OneWayBind(
                    this.ViewModel,
                    vm => vm.IsServiceRunning,
                    v => v.StartServiceButton.IsVisible,
                    value => !value)
                    .DisposeWith(disposables);

                this.OneWayBind(
                    this.ViewModel,
                    vm => vm.IsServiceRunning,
                    v => v.StopServiceButton.IsVisible)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.StartService, v => v.StartServiceButton)
                    .DisposeWith(disposables);

                this.BindCommand(this.ViewModel, vm => vm.StopService, v => v.StopServiceButton)
                    .DisposeWith(disposables);
            });

            this.InitializeComponent();
        }

        private TextBlock ServiceRunningTextBlock { get; set; } = null!;
        private TextBlock ServiceNotRunningTextBlock { get; set; } = null!;
        private Button StartServiceButton { get; set; } = null!;
        private Button StopServiceButton { get; set; } = null!;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.ServiceRunningTextBlock = this.FindControl<TextBlock>(nameof(this.ServiceRunningTextBlock));
            this.ServiceNotRunningTextBlock = this.FindControl<TextBlock>(nameof(this.ServiceNotRunningTextBlock));
            this.StartServiceButton = this.FindControl<Button>(nameof(this.StartServiceButton));
            this.StopServiceButton = this.FindControl<Button>(nameof(this.StopServiceButton));
        }
    }
}

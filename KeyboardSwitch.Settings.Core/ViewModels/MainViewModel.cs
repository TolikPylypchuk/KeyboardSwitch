using System.Reactive;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel(ServiceViewModel? serviceViewModel = null)
        {
            this.ServiceViewModel = serviceViewModel ?? new ServiceViewModel();
            this.OpenExternally = ReactiveCommand.Create(() => { });
        }

        public ServiceViewModel ServiceViewModel { get; }

        public ReactiveCommand<Unit, Unit> OpenExternally { get; set; }
    }
}

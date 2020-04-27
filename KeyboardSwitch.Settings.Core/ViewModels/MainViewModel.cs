using System.Reactive;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel(MainContentViewModel? mainContent = null, ServiceViewModel? serviceViewModel = null)
        {
            this.MainContent = mainContent ?? new MainContentViewModel();
            this.ServiceViewModel = serviceViewModel ?? new ServiceViewModel();

            this.OpenExternally = ReactiveCommand.Create(() => { });
        }

        public MainContentViewModel MainContent { get; }
        public ServiceViewModel ServiceViewModel { get; }

        public ReactiveCommand<Unit, Unit> OpenExternally { get; set; }
    }
}

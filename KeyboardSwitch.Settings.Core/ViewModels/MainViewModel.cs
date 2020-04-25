using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel(ServiceViewModel? serviceViewModel = null)
        {
            this.ServiceViewModel = serviceViewModel ?? Locator.Current.GetService<ServiceViewModel>();
        }

        public ServiceViewModel ServiceViewModel { get; }
    }
}

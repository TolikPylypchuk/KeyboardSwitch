using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class MainContentViewModel : ReactiveObject
    {
        public MainContentViewModel(CharMappingModel charMappingModel, PreferencesModel preferencesModel)
        {
            this.CharMappingViewModel = new CharMappingViewModel(charMappingModel);
            this.PreferencesViewModel = new PreferencesViewModel(preferencesModel);
        }

        public CharMappingViewModel CharMappingViewModel { get; }
        public PreferencesViewModel PreferencesViewModel { get; }
    }
}

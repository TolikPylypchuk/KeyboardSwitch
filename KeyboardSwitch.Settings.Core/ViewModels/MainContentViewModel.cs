using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class MainContentViewModel : ReactiveObject
    {
        public MainContentViewModel(
            CharMappingViewModel? switchSettings = null,
            PreferencesViewModel? otherSettings = null)
        {
            this.SwitchSettings = switchSettings ?? new CharMappingViewModel();
            this.OtherSettings = otherSettings ?? new PreferencesViewModel();
        }

        public CharMappingViewModel SwitchSettings { get; }
        public PreferencesViewModel OtherSettings { get; }
    }
}

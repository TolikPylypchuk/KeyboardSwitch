using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class MainContentViewModel : ReactiveObject
    {
        public MainContentViewModel(
            SwitchSettingsViewModel? switchSettings = null,
            OtherSettingsViewModel? otherSettings = null)
        {
            this.SwitchSettings = switchSettings ?? new SwitchSettingsViewModel();
            this.OtherSettings = otherSettings ?? new OtherSettingsViewModel();
        }

        public SwitchSettingsViewModel SwitchSettings { get; }
        public OtherSettingsViewModel OtherSettings { get; }
    }
}

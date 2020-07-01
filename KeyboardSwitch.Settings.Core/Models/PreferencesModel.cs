using KeyboardSwitch.Common.Settings;

namespace KeyboardSwitch.Settings.Core.Models
{
    public sealed class PreferencesModel
    {
        public PreferencesModel(AppSettings appSettings, bool startup)
        {
            this.SwitchMode = appSettings.SwitchMode;
            this.HotKeySwitchSettings = appSettings.HotKeySwitchSettings;
            this.ModifierKeysSwitchSettings = appSettings.ModifierKeysSwitchSettings;
            this.InstantSwitching = appSettings.InstantSwitching;
            this.SwitchLayout = appSettings.SwitchLayout;
            this.Startup = startup;
            this.ShowUninstalledLayoutsMessage = appSettings.ShowUninstalledLayoutsMessage;
        }

        public SwitchMode SwitchMode { get; set; }
        public HotKeySwitchSettings HotKeySwitchSettings { get; set; }
        public ModifierKeysSwitchSettings ModifierKeysSwitchSettings { get; set; }

        public bool InstantSwitching { get; set; }
        public bool SwitchLayout { get; set; }
        public bool Startup { get; set; }
        public bool ShowUninstalledLayoutsMessage { get; set; }
    }
}

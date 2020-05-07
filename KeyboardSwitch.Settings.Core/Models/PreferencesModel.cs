using KeyboardSwitch.Common.Settings;

namespace KeyboardSwitch.Settings.Core.Models
{
    public sealed class PreferencesModel
    {
        public PreferencesModel(AppSettings appSettings)
        {
            this.SwitchMode = appSettings.SwitchMode;
            this.HotKeySwitchSettings = appSettings.HotKeySwitchSettings;
            this.ModifierKeysSwitchSettings = appSettings.ModifierKeysSwitchSettings;
        }

        public SwitchMode SwitchMode { get; set; }
        public HotKeySwitchSettings HotKeySwitchSettings { get; set; }
        public ModifierKeysSwitchSettings ModifierKeysSwitchSettings { get; set; }
    }
}

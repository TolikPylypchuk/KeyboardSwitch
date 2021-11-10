namespace KeyboardSwitch.Settings.Core.Models;

public sealed class PreferencesModel
{
    public PreferencesModel(AppSettings appSettings, bool startup)
    {
        this.SwitchSettings = appSettings.SwitchSettings;
        this.InstantSwitching = appSettings.InstantSwitching;
        this.SwitchLayout = appSettings.SwitchLayout;
        this.Startup = startup;
        this.ShowUninstalledLayoutsMessage = appSettings.ShowUninstalledLayoutsMessage;
        this.ShowConverter = appSettings.ShowConverter;
    }

    public SwitchSettings SwitchSettings { get; set; }

    public bool InstantSwitching { get; set; }
    public bool SwitchLayout { get; set; }
    public bool Startup { get; set; }
    public bool ShowUninstalledLayoutsMessage { get; set; }
    public bool ShowConverter { get; set; }
}

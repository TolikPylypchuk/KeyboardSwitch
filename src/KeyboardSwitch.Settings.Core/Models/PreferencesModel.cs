namespace KeyboardSwitch.Settings.Core.Models;

public sealed class PreferencesModel(AppSettings appSettings, bool startup)
{
    public SwitchSettings SwitchSettings { get; set; } = appSettings.SwitchSettings;

    public bool InstantSwitching { get; set; } = appSettings.InstantSwitching;
    public bool SwitchLayout { get; set; } = appSettings.SwitchLayout;
    public bool Startup { get; set; } = startup;
    public bool ShowUninstalledLayoutsMessage { get; set; } = appSettings.ShowUninstalledLayoutsMessage;
    public bool UseXsel { get; set; } = appSettings.UseXsel;

    public AppTheme AppTheme { get; set; } = appSettings.AppTheme;
    public AppThemeVariant AppThemeVariant { get; set; } = appSettings.AppThemeVariant;
}

namespace KeyboardSwitch.Core.Settings;

public sealed class AppSettings
{
    public SwitchSettings SwitchSettings { get; set; } = null!;

    public Dictionary<string, string> CharsByKeyboardLayoutId { get; set; } = [];

    public bool InstantSwitching { get; set; }

    public bool SwitchLayout { get; set; }

    public bool ShowUninstalledLayoutsMessage { get; set; }

    public Version AppVersion { get; set; } = null!;
}

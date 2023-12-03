namespace KeyboardSwitch.Core.Settings;

[DataContract]
public sealed class AppSettings
{
    public static readonly string CacheKey = "AppSettings";

    [DataMember]
    public SwitchSettings SwitchSettings { get; set; } = null!;

    [DataMember]
    public Dictionary<string, string> CharsByKeyboardLayoutId { get; set; } = new();

    [DataMember]
    public bool InstantSwitching { get; set; }

    [DataMember]
    public bool SwitchLayout { get; set; }

    [DataMember]
    public bool ShowUninstalledLayoutsMessage { get; set; }

    [DataMember]
    public Version AppVersion { get; set; } = null!;
}

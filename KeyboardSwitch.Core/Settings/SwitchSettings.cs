namespace KeyboardSwitch.Core.Settings;

[DataContract]
public sealed class SwitchSettings
{
    [DataMember]
    public List<ModifierMask> ForwardModifiers { get; set; } = new();

    [DataMember]
    public List<ModifierMask> BackwardModifiers { get; set; } = new();

    [DataMember]
    public int PressCount { get; set; }

    [DataMember]
    public int WaitMilliseconds { get; set; }

    [DataMember]
    public List<KeyCode> LayoutForwardKeys { get; set; } = new();

    [DataMember]
    public List<KeyCode> LayoutBackwardKeys { get; set; } = new();
}

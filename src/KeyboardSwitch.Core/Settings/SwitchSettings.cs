namespace KeyboardSwitch.Core.Settings;

public sealed class SwitchSettings
{
    public List<ModifierMask> ForwardModifiers { get; set; } = [];
    public List<ModifierMask> BackwardModifiers { get; set; } = [];

    public int PressCount { get; set; }
    public int WaitMilliseconds { get; set; }
}

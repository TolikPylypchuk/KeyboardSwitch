namespace KeyboardSwitch.Core.Settings;

public sealed record SwitchSettings
{
    public required List<ModifierMask> ForwardModifiers { get; init; }
    public required List<ModifierMask> BackwardModifiers { get; init; }

    public required int PressCount { get; init; }
    public required int WaitMilliseconds { get; init; }
}

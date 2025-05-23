namespace KeyboardSwitch.Core.Settings;

public sealed record SwitchSettings
{
    public required List<EventMask> ForwardModifiers { get; init; }
    public required List<EventMask> BackwardModifiers { get; init; }

    public required int PressCount { get; init; }
    public required int WaitMilliseconds { get; init; }
}

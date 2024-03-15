namespace KeyboardSwitch.Core.Settings;

public sealed record AppSettings
{
    public required SwitchSettings SwitchSettings { get; init; }

    public required ImmutableDictionary<string, string> CharsByKeyboardLayoutId { get; init; }

    public required bool InstantSwitching { get; init; }

    public required bool SwitchLayout { get; init; }

    public required bool ShowUninstalledLayoutsMessage { get; init; }

    public required Version AppVersion { get; init; }
}

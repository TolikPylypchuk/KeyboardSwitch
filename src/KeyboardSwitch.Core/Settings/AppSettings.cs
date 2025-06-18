namespace KeyboardSwitch.Core.Settings;

public enum AppTheme
{
    Fluent,
    MacOS,
    Simple
}

public enum AppThemeVariant
{
    Auto,
    Light,
    Dark
}

public sealed record AppSettings
{
    public required SwitchSettings SwitchSettings { get; init; }

    public required ImmutableDictionary<string, string> CharsByKeyboardLayoutId { get; init; }

    public required bool InstantSwitching { get; init; }

    public required bool SwitchLayout { get; init; }

    public required bool ShowUninstalledLayoutsMessage { get; init; }

    public required bool UseXsel { get; init; }

    public required Version AppVersion { get; init; }

    public AppTheme AppTheme { get; init; } = AppTheme.Fluent;

    public AppThemeVariant AppThemeVariant { get; init; } = AppThemeVariant.Auto;
}

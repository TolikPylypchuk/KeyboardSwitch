namespace KeyboardSwitch;

public enum ExitCode
{
    Success = 0,
    Error = 1,
    IncompatibleSettingsVersion = 2,
    KeyboardSwitchNotRunning = 3,
    UnknownCommand = 4,
    SettingsDoNotExist = 5,
    MacOSAccessibilityDisabled = 6
}

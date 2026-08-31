namespace KeyboardSwitch.Linux;

public static class LinuxSessionDetector
{
    public static bool IsRunningOnWayland =>
        Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is { Length: > 0 };
}

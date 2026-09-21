namespace KeyboardSwitch.Linux;

public static class SessionDetector
{
    private const string WaylandDisplayVariable = "WAYLAND_DISPLAY";
    private const string CurrentDesktopVariable = "XDG_CURRENT_DESKTOP";

    public static bool IsRunningOnWayland =>
        Environment.GetEnvironmentVariable(WaylandDisplayVariable) is { Length: > 0 };

    public static DesktopEnvironment CurrentDesktopEnvironment =>
        Environment.GetEnvironmentVariable(CurrentDesktopVariable) switch
        {
            string desktop when Contains(desktop, "gnome") || Contains(desktop, "unity") =>
                DesktopEnvironment.Gnome,
            string desktop when Contains(desktop, "kde") => DesktopEnvironment.Kde,
            _ => DesktopEnvironment.Other
        };

    private static bool Contains(string currentDesktop, string desktopEnvironment) =>
        currentDesktop.Contains(desktopEnvironment, StringComparison.OrdinalIgnoreCase);
}

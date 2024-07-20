using System.Text.RegularExpressions;

namespace KeyboardSwitch.Linux;

internal static partial class GnomeDetector
{
    public static bool IsRunningOnGnome()
    {
        var currentDesktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
        return currentDesktopEnvironment != null &&
            (currentDesktopEnvironment.Contains("gnome", StringComparison.CurrentCultureIgnoreCase) ||
            currentDesktopEnvironment.Contains("unity", StringComparison.CurrentCultureIgnoreCase));
    }

    public static Version? TryGetGnomeVersion()
    {
        if (!IsRunningOnGnome())
        {
            return null;
        }

        var gnomeShell = Process.Start(
            new ProcessStartInfo() { FileName = "gnome-shell", Arguments = "--version", RedirectStandardOutput = true });

        if (gnomeShell is null)
        {
            return null;
        }

        var version = gnomeShell.StandardOutput.ReadToEnd();
        var match = GetGnomeShellVersionRegex().Match(version);

        return match.Success ? Version.Parse(match.Value) : null;
    }

    [GeneratedRegex(@"[0-9]+(:?\.[0-9]+)?")]
    private static partial Regex GetGnomeShellVersionRegex();
}

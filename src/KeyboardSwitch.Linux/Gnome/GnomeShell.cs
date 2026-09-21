using System.Text.RegularExpressions;

namespace KeyboardSwitch.Linux.Gnome;

internal static partial class GnomeShell
{
    public static Version? TryGetVersion()
    {
        if (SessionDetector.CurrentDesktopEnvironment != DesktopEnvironment.Gnome)
        {
            return null;
        }

        var gnomeShell = Process.Start(
            new ProcessStartInfo()
            {
                FileName = "gnome-shell",
                Arguments = "--version",
                RedirectStandardOutput = true
            });

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

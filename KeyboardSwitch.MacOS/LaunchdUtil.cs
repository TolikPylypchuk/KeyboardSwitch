namespace KeyboardSwitch.MacOS;

internal static class LaunchdUtil
{
    public const string LaunchCtl = "launchctl";

    public static int? GetCurrentUserId()
    {
        var id = Process.Start(new ProcessStartInfo("id", "-u") { RedirectStandardOutput = true });

        if (id != null)
        {
            string output = id.StandardOutput.ReadToEnd();
            return Int32.TryParse(output.Trim() ?? String.Empty, out int result) ? result : null;
        }

        return null;
    }
}

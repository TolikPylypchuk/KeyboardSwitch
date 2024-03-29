using System.Diagnostics;

namespace KeyboardSwitch.Core.Services.Users;

public sealed class PosixUserProvider : IUserProvider
{
    public string? GetCurrentUser()
    {
        var id = Process.Start(new ProcessStartInfo("id", "-u") { RedirectStandardOutput = true });
        return id?.StandardOutput.ReadToEnd()?.Trim();
    }
}

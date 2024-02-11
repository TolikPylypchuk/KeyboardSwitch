namespace KeyboardSwitch.Windows.Services;

internal sealed class WinUserProvider : IUserProvider
{
    public string? GetCurrentUser()
    {
        var userName = Environment.GetEnvironmentVariable("USERNAME");
        var userDomain = Environment.GetEnvironmentVariable("USERDOMAIN");

        return String.IsNullOrEmpty(userDomain) ? userName : $"{userDomain}/{userName}";
    }
}

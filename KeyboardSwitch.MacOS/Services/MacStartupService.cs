namespace KeyboardSwitch.MacOS.Services;

internal class MacStartupService : IStartupService
{
    public void ConfigureStartup(bool startup)
    { }

    public bool IsStartupConfigured() =>
        false;
}

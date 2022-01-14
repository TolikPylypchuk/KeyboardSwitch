namespace KeyboardSwitch.MacOS.Services;

internal class MacStartupService : IStartupService
{
    public void ConfigureStartup(AppSettings settings, bool startup)
    { }

    public bool IsStartupConfigured(AppSettings settings) =>
        false;
}

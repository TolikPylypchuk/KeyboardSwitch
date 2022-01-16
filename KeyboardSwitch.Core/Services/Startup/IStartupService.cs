namespace KeyboardSwitch.Core.Services.Startup;

public interface IStartupService
{
    bool IsStartupConfigured();
    void ConfigureStartup(bool startup);
}

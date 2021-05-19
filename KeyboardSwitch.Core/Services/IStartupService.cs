using KeyboardSwitch.Core.Settings;

namespace KeyboardSwitch.Core.Services
{
    public interface IStartupService
    {
        bool IsStartupConfigured(AppSettings settings);
        void ConfigureStartup(AppSettings settings, bool startup);
    }
}

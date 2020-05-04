using System.Threading.Tasks;

using KeyboardSwitch.Common.Settings;

namespace KeyboardSwitch.Common.Services
{
    public interface ISettingsService
    {
        ValueTask<AppSettings> GetAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings appSettings);

        void InvalidateSwitchSettings();
    }
}

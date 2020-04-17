using System.Threading.Tasks;

using KeyboardSwitch.Common.Settings;

namespace KeyboardSwitch.Common.Services
{
    public interface ISettingsService
    {
        Task<SwitchSettings> GetSwitchSettingsAsync();
        Task SaveSwitchSettingsAsync(SwitchSettings switchSettings);
    }
}

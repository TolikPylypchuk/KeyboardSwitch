using System.Threading.Tasks;

using KeyboardSwitch.Common.Settings;

namespace KeyboardSwitch.Common.Services
{
    public interface ISettingsService
    {
        ValueTask<SwitchSettings> GetSwitchSettingsAsync();
        Task SaveSwitchSettingsAsync(SwitchSettings switchSettings);

        ValueTask<UISettings> GetUISettingsAsync();
        Task SaveUISettingsAsync(UISettings uiSettings);

        void InvalidateSwitchSettings();
    }
}

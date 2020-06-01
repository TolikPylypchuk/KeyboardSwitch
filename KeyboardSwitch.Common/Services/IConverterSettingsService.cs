using System.Threading.Tasks;

using KeyboardSwitch.Common.Settings;

namespace KeyboardSwitch.Common.Services
{
    public interface IConverterSettingsService
    {
        ValueTask<ConverterSettings> GetConverterSettingsAsync();
        Task SaveConverterSettingsAsync(ConverterSettings appSettings);
    }
}

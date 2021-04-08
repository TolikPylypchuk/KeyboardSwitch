using System.Threading.Tasks;

using KeyboardSwitch.Core.Settings;

namespace KeyboardSwitch.Core.Services
{
    public interface IConverterSettingsService
    {
        ValueTask<ConverterSettings> GetConverterSettingsAsync();
        Task SaveConverterSettingsAsync(ConverterSettings appSettings);
    }
}

namespace KeyboardSwitch.Core.Services.Settings;

public interface IConverterSettingsService
{
    ValueTask<ConverterSettings> GetConverterSettingsAsync();
    Task SaveConverterSettingsAsync(ConverterSettings appSettings);
}

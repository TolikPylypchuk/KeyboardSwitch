namespace KeyboardSwitch.Core.Services.Settings;

public interface IAppSettingsService
{
    IObservable<Unit> SettingsInvalidated { get; }

    Task<AppSettings> GetAppSettingsAsync();
    Task SaveAppSettingsAsync(AppSettings appSettings);

    void InvalidateAppSettings();
}

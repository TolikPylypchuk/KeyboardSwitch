namespace KeyboardSwitch.Core.Services.Settings;

public interface IAppSettingsService
{
    IObservable<Unit> SettingsInvalidated { get; }

    Task<AppSettings> GetAppSettings();
    Task SaveAppSettings(AppSettings appSettings);

    void InvalidateAppSettings();
}

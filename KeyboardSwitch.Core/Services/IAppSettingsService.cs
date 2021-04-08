using System;
using System.Reactive;
using System.Threading.Tasks;

using KeyboardSwitch.Core.Settings;

namespace KeyboardSwitch.Core.Services
{
    public interface IAppSettingsService
    {
        IObservable<Unit> SettingsInvalidated { get; }

        Task<AppSettings> GetAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings appSettings);

        void InvalidateAppSettings();
    }
}

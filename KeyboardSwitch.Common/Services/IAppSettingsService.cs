using System;
using System.Reactive;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Settings;

namespace KeyboardSwitch.Common.Services
{
    public interface IAppSettingsService
    {
        IObservable<Unit> SettingsInvalidated { get; }

        ValueTask<AppSettings> GetAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings appSettings);

        void InvalidateAppSettings();
    }
}

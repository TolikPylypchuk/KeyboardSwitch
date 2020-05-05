using System;
using System.Reactive;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Settings;

namespace KeyboardSwitch.Common.Services
{
    public interface ISettingsService
    {
        IObservable<Unit> SettingsInvalidated { get; }

        ValueTask<AppSettings> GetAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings appSettings);

        void InvalidateAppSettings();
    }
}

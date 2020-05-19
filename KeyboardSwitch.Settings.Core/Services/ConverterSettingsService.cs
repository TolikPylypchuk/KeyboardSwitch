using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using static KeyboardSwitch.Settings.Core.Constants;

namespace KeyboardSwitch.Settings.Core.Services
{
    public class ConverterSettingsService : ReactiveObject, ISettingsService
    {
        private readonly Subject<Unit> settingsInvalidated = new Subject<Unit>();

        public ConverterSettingsService(
            IObservable<CustomLayoutModel> sourceLayout,
            IObservable<CustomLayoutModel> targetLayout)
        {
            sourceLayout.ToPropertyEx(this, vm => vm.SourceLayout);
            targetLayout.ToPropertyEx(this, vm => vm.TargetLayout);
        }

        private CustomLayoutModel SourceLayout { [ObservableAsProperty]get; } = null!;
        private CustomLayoutModel TargetLayout { [ObservableAsProperty]get; } = null!;

        public IObservable<Unit> SettingsInvalidated
            => this.settingsInvalidated.AsObservable();

        public ValueTask<AppSettings> GetAppSettingsAsync()
            => new ValueTask<AppSettings>(new AppSettings
            {
                CharsByKeyboardLayoutId = new Dictionary<int, string>
                {
                    [SourceLayoutId] = this.SourceLayout.Chars,
                    [TargetLayoutId] = this.TargetLayout.Chars
                },
                InstantSwitching = false,
                SwitchLayout = false
            });

        public void InvalidateAppSettings()
            => this.settingsInvalidated.OnNext(Unit.Default);

        public Task SaveAppSettingsAsync(AppSettings appSettings)
            => Task.CompletedTask;
    }
}

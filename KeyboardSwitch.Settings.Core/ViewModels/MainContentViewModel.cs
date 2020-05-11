using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class MainContentViewModel : ReactiveObject
    {
        private readonly ISettingsService settingsService;

        public MainContentViewModel(
            CharMappingModel charMappingModel,
            PreferencesModel preferencesModel,
            ISettingsService? settingsService = null)
        {
            this.CharMappingViewModel = new CharMappingViewModel(charMappingModel);
            this.PreferencesViewModel = new PreferencesViewModel(preferencesModel);

            this.settingsService = settingsService ?? Locator.Current.GetService<ISettingsService>();

            this.Save = ReactiveCommand.CreateFromTask(this.SaveAsync);

            this.CharMappingViewModel.Save.Discard()
                .Merge(this.PreferencesViewModel.Save.Discard())
                .InvokeCommand(this.Save);
        }

        public CharMappingViewModel CharMappingViewModel { get; }
        public PreferencesViewModel PreferencesViewModel { get; }

        public ReactiveCommand<Unit, Unit> Save { get; }

        private async Task SaveAsync()
        {
            var settings = await this.settingsService.GetAppSettingsAsync();

            settings.CharsByKeyboardLayoutId = this.CharMappingViewModel
                .CharMappingModel
                .Layouts
                .ToDictionary(
                    layout => layout.Id,
                    layout => new string(layout.Chars.Select(ch => ch.Character).ToArray()));

            settings.SwitchMode = this.PreferencesViewModel.SwitchMode;

            settings.HotKeySwitchSettings =
                this.PreferencesViewModel.HotKeySwitchViewModel.HotKeySwitchSettings;

            settings.ModifierKeysSwitchSettings =
                this.PreferencesViewModel.ModifierKeysSwitchModel.ModifierKeysSwitchSettings;

            await this.settingsService.SaveAppSettingsAsync(settings);
        }
    }
}

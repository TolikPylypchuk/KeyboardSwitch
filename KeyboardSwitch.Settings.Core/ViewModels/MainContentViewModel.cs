using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class MainContentViewModel : ReactiveObject
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly IConverterSettingsService converterSettingsService;

        public MainContentViewModel(
            CharMappingModel charMappingModel,
            PreferencesModel preferencesModel,
            ConverterModel converterModel,
            IAppSettingsService? appSettingsService = null,
            IConverterSettingsService? converterSettingsService = null)
        {
            this.CharMappingViewModel = new CharMappingViewModel(charMappingModel);
            this.PreferencesViewModel = new PreferencesViewModel(preferencesModel);
            this.ConverterViewModel = new ConverterViewModel(converterModel);
            this.ConverterSettingsViewModel = new ConverterSettingsViewModel(converterModel);

            this.appSettingsService = appSettingsService ?? Locator.Current.GetService<IAppSettingsService>();
            this.converterSettingsService =
                converterSettingsService ?? Locator.Current.GetService<IConverterSettingsService>();

            this.SaveCharMappingSettings = ReactiveCommand.CreateFromTask<CharMappingModel>(
                this.SaveCharMappingSettingsAsync);
            this.SavePreferences = ReactiveCommand.CreateFromTask<PreferencesModel>(
                this.SavePreferencesAsync);
            this.SaveConverterSettings = ReactiveCommand.CreateFromTask<ConverterModel>(
                this.SaveConverterSettingsAsync);

            this.CharMappingViewModel.Save.InvokeCommand(this.SaveCharMappingSettings);
            this.PreferencesViewModel.Save.InvokeCommand(this.SavePreferences);
            this.ConverterSettingsViewModel.Save.InvokeCommand(this.SaveConverterSettings);
        }

        public CharMappingViewModel CharMappingViewModel { get; }
        public PreferencesViewModel PreferencesViewModel { get; }
        public ConverterViewModel ConverterViewModel { get; }
        public ConverterSettingsViewModel ConverterSettingsViewModel { get; }

        public ReactiveCommand<CharMappingModel, Unit> SaveCharMappingSettings { get; }
        public ReactiveCommand<PreferencesModel, Unit> SavePreferences { get; }
        public ReactiveCommand<ConverterModel, Unit> SaveConverterSettings { get; }

        private async Task SaveCharMappingSettingsAsync(CharMappingModel charMappingModel)
        {
            var settings = await this.appSettingsService.GetAppSettingsAsync();

            int maxLength = charMappingModel.Layouts.Max(layout => layout.Chars.Length);

            settings.CharsByKeyboardLayoutId = charMappingModel.Layouts
                .ToDictionary(layout => layout.Id, layout => layout.Chars.PadRight(maxLength));

            await this.appSettingsService.SaveAppSettingsAsync(settings);
        }

        private async Task SavePreferencesAsync(PreferencesModel preferencesModel)
        {
            var settings = await this.appSettingsService.GetAppSettingsAsync();

            settings.SwitchMode = this.PreferencesViewModel.SwitchMode;
            settings.HotKeySwitchSettings = preferencesModel.HotKeySwitchSettings;
            settings.ModifierKeysSwitchSettings = preferencesModel.ModifierKeysSwitchSettings;
            settings.InstantSwitching = preferencesModel.InstantSwitching;
            settings.SwitchLayout = preferencesModel.SwitchLayout;

            await this.appSettingsService.SaveAppSettingsAsync(settings);
        }

        private async Task SaveConverterSettingsAsync(ConverterModel converterModel)
        {
            var settings = await this.converterSettingsService.GetConverterSettingsAsync();

            settings.Layouts = converterModel.Layouts
                .Select(layout => new CustomLayoutSettings
                {
                    Id = layout.Id,
                    Name = layout.Name,
                    Chars = layout.Chars
                })
                .ToList();

            await this.converterSettingsService.SaveConverterSettingsAsync(settings);
        }
    }
}

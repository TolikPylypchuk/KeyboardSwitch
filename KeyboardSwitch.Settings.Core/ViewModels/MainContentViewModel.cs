using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class MainContentViewModel : ReactiveObject
    {
        private readonly IAppSettingsService appSettingsService;
        private readonly IConverterSettingsService converterSettingsService;
        private readonly IStartupService startupService;

        private readonly BehaviorSubject<bool> removeLayoutsEnabled;

        public MainContentViewModel(
            CharMappingModel charMappingModel,
            PreferencesModel preferencesModel,
            ConverterModel converterModel,
            IAppSettingsService? appSettingsService = null,
            IConverterSettingsService? converterSettingsService = null,
            IStartupService? startupService = null)
        {
            this.removeLayoutsEnabled = new(preferencesModel.ShowUninstalledLayoutsMessage);

            this.appSettingsService = appSettingsService ?? Locator.Current.GetService<IAppSettingsService>();
            this.converterSettingsService =
                converterSettingsService ?? Locator.Current.GetService<IConverterSettingsService>();
            this.startupService = startupService ?? Locator.Current.GetService<IStartupService>();

            this.CharMappingViewModel = new(charMappingModel, this.removeLayoutsEnabled);
            this.PreferencesViewModel = new(preferencesModel);
            this.ConverterViewModel = new(converterModel);
            this.ConverterSettingsViewModel = new(converterModel);
            this.AboutViewModel = new();

            this.SaveCharMappingSettings = ReactiveCommand.CreateFromTask<CharMappingModel>(
                this.SaveCharMappingSettingsAsync);
            this.SavePreferences = ReactiveCommand.CreateFromTask<PreferencesModel>(
                this.SavePreferencesAsync);
            this.SaveConverterSettings = ReactiveCommand.CreateFromTask<ConverterModel>(
                this.SaveConverterSettingsAsync);
            this.OpenAboutTab = ReactiveCommand.Create(() => { });

            this.CharMappingViewModel.Save.InvokeCommand(this.SaveCharMappingSettings);
            this.PreferencesViewModel.Save.InvokeCommand(this.SavePreferences);
            this.ConverterSettingsViewModel.Save.InvokeCommand(this.SaveConverterSettings);

            this.PreferencesViewModel.Save
                .Select(model => model.ShowUninstalledLayoutsMessage)
                .Subscribe(this.removeLayoutsEnabled);

            this.ShowConverter = preferencesModel.ShowConverter;
        }

        public CharMappingViewModel CharMappingViewModel { get; }
        public PreferencesViewModel PreferencesViewModel { get; }
        public ConverterViewModel ConverterViewModel { get; }
        public ConverterSettingsViewModel ConverterSettingsViewModel { get; }
        public AboutViewModel AboutViewModel { get; }

        public ReactiveCommand<CharMappingModel, Unit> SaveCharMappingSettings { get; }
        public ReactiveCommand<PreferencesModel, Unit> SavePreferences { get; }
        public ReactiveCommand<ConverterModel, Unit> SaveConverterSettings { get; }
        public ReactiveCommand<Unit, Unit> OpenAboutTab { get; }

        [Reactive]
        public bool ShowConverter { get; private set; }

        private async Task SaveCharMappingSettingsAsync(CharMappingModel charMappingModel)
        {
            var settings = await this.appSettingsService.GetAppSettingsAsync();

            int maxLength = charMappingModel.Layouts.Max(layout => layout.Chars.Length);

            settings.CharsByKeyboardLayoutId = charMappingModel.Layouts
                .ToDictionary(layout => layout.Id, layout => layout.Chars.PadRight(maxLength));

            if (charMappingModel.ShouldRemoveLayouts)
            {
                foreach (var id in charMappingModel.RemovableLayoutIds)
                {
                    settings.CharsByKeyboardLayoutId.Remove(id);
                }

                charMappingModel.ShouldRemoveLayouts = false;
                charMappingModel.RemovableLayoutIds.Clear();
            }

            await this.appSettingsService.SaveAppSettingsAsync(settings);
        }

        private async Task SavePreferencesAsync(PreferencesModel preferencesModel)
        {
            var settings = await this.appSettingsService.GetAppSettingsAsync();

            settings.SwitchSettings = preferencesModel.SwitchSettings;
            settings.InstantSwitching = preferencesModel.InstantSwitching;
            settings.SwitchLayout = preferencesModel.SwitchLayout;
            settings.ShowUninstalledLayoutsMessage = preferencesModel.ShowUninstalledLayoutsMessage;
            settings.ShowConverter = preferencesModel.ShowConverter;

            await this.appSettingsService.SaveAppSettingsAsync(settings);

            if (this.startupService.IsStartupConfigured() != preferencesModel.Startup)
            {
                await this.startupService.ConfigureStartupAsync(preferencesModel.Startup);
            }

            this.ShowConverter = preferencesModel.ShowConverter;
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

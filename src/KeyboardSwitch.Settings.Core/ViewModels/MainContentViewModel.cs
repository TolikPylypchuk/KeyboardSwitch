using System.Collections.Immutable;

namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed partial class MainContentViewModel : ReactiveObject
{
    private readonly IAppSettingsService appSettingsService;
    private readonly IStartupService startupService;

    private readonly BehaviorSubject<bool> removeLayoutsEnabled;

    public MainContentViewModel(
        CharMappingModel charMappingModel,
        PreferencesModel preferencesModel,
        IAppSettingsService? appSettingsService = null,
        IStartupService? startupService = null)
    {
        this.removeLayoutsEnabled = new(preferencesModel.ShowUninstalledLayoutsMessage);

        this.appSettingsService = appSettingsService ?? AppLocator.Current.GetRequiredService<IAppSettingsService>();
        this.startupService = startupService ?? AppLocator.Current.GetRequiredService<IStartupService>();

        this.CharMappingViewModel = new(charMappingModel, this.removeLayoutsEnabled);
        this.PreferencesViewModel = new(preferencesModel);
        this.AboutViewModel = new();

        this.CharMappingViewModel.SaveCommand.InvokeCommand(this.SaveCharMappingSettingsCommand);
        this.PreferencesViewModel.SaveCommand.InvokeCommand(this.SavePreferencesCommand);

        this.PreferencesViewModel.SaveCommand
            .Select(model => model.ShowUninstalledLayoutsMessage)
            .Subscribe(this.removeLayoutsEnabled);
    }

    public CharMappingViewModel CharMappingViewModel { get; }
    public PreferencesViewModel PreferencesViewModel { get; }
    public AboutViewModel AboutViewModel { get; }

    [ReactiveCommand]
    private async Task SaveCharMappingSettings(CharMappingModel charMappingModel)
    {
        var settings = await this.appSettingsService.GetAppSettings();

        int maxLength = charMappingModel.Layouts.Max(layout => layout.Chars.Length);

        var mappings = charMappingModel.Layouts
            .ToDictionary(layout => layout.Id, layout => layout.Chars.PadRight(maxLength));

        if (charMappingModel.ShouldRemoveLayouts)
        {
            foreach (var id in charMappingModel.RemovableLayoutIds)
            {
                mappings.Remove(id);
            }

            charMappingModel.ShouldRemoveLayouts = false;
            charMappingModel.RemovableLayoutIds.Clear();
        }

        var newSettings = settings with { CharsByKeyboardLayoutId = mappings.ToImmutableDictionary() };
        await this.appSettingsService.SaveAppSettings(newSettings);
    }

    [ReactiveCommand]
    private async Task<PreferencesModel> SavePreferences(PreferencesModel preferencesModel)
    {
        var settings = await this.appSettingsService.GetAppSettings();

        var newSettings = settings with
        {
            SwitchSettings = preferencesModel.SwitchSettings,
            InstantSwitching = preferencesModel.InstantSwitching,
            SwitchLayout = preferencesModel.SwitchLayout,
            ShowUninstalledLayoutsMessage = preferencesModel.ShowUninstalledLayoutsMessage,
            UseXsel = preferencesModel.UseXsel,
            AppTheme = preferencesModel.AppTheme,
            AppThemeVariant = preferencesModel.AppThemeVariant
        };

        await this.appSettingsService.SaveAppSettings(newSettings);

        if (this.startupService.IsStartupConfigured() != preferencesModel.Startup)
        {
            this.startupService.ConfigureStartup(preferencesModel.Startup);
        }

        return preferencesModel;
    }

    [ReactiveCommand]
    private void OpenAboutTab()
    { }
}

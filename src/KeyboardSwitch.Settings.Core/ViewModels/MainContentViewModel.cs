namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed class MainContentViewModel : ReactiveObject
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

        this.appSettingsService = appSettingsService ?? GetDefaultService<IAppSettingsService>();
        this.startupService = startupService ?? GetDefaultService<IStartupService>();

        this.CharMappingViewModel = new(charMappingModel, this.removeLayoutsEnabled);
        this.PreferencesViewModel = new(preferencesModel);
        this.AboutViewModel = new();

        this.SaveCharMappingSettings = ReactiveCommand.CreateFromTask<CharMappingModel>(
            this.SaveCharMappingSettingsAsync);
        this.SavePreferences = ReactiveCommand.CreateFromTask<PreferencesModel>(
            this.SavePreferencesAsync);
        this.OpenAboutTab = ReactiveCommand.Create(() => { });

        this.CharMappingViewModel.Save.InvokeCommand(this.SaveCharMappingSettings);
        this.PreferencesViewModel.Save.InvokeCommand(this.SavePreferences);

        this.PreferencesViewModel.Save
            .Select(model => model.ShowUninstalledLayoutsMessage)
            .Subscribe(this.removeLayoutsEnabled);
    }

    public CharMappingViewModel CharMappingViewModel { get; }
    public PreferencesViewModel PreferencesViewModel { get; }
    public AboutViewModel AboutViewModel { get; }

    public ReactiveCommand<CharMappingModel, Unit> SaveCharMappingSettings { get; }
    public ReactiveCommand<PreferencesModel, Unit> SavePreferences { get; }
    public ReactiveCommand<Unit, Unit> OpenAboutTab { get; }

    private async Task SaveCharMappingSettingsAsync(CharMappingModel charMappingModel)
    {
        var settings = await this.appSettingsService.GetAppSettings();

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

        await this.appSettingsService.SaveAppSettings(settings);
    }

    private async Task SavePreferencesAsync(PreferencesModel preferencesModel)
    {
        var settings = await this.appSettingsService.GetAppSettings();

        settings.SwitchSettings = preferencesModel.SwitchSettings;
        settings.InstantSwitching = preferencesModel.InstantSwitching;
        settings.SwitchLayout = preferencesModel.SwitchLayout;
        settings.ShowUninstalledLayoutsMessage = preferencesModel.ShowUninstalledLayoutsMessage;

        await this.appSettingsService.SaveAppSettings(settings);

        if (this.startupService.IsStartupConfigured() != preferencesModel.Startup)
        {
            this.startupService.ConfigureStartup(preferencesModel.Startup);
        }
    }
}

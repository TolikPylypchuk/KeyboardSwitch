using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed partial class MainViewModel : ReactiveObject
{
    private readonly Subject<PreferencesModel> preferencesSaved = new();

    public MainViewModel(
        AppSettings appSettings,
        ILayoutService? layoutService = null,
        IStartupService? startupService = null)
    {
        layoutService ??= AppLocator.Current.GetRequiredService<ILayoutService>();
        startupService ??= AppLocator.Current.GetRequiredService<IStartupService>();

        this.MainContentViewModel = new MainContentViewModel(
            this.CreateCharMappingModel(appSettings, layoutService.GetKeyboardLayouts()),
            new PreferencesModel(appSettings, startupService.IsStartupConfigured()));

        this.ServiceViewModel = new ServiceViewModel();

        this.MainContentViewModel.SaveCharMappingSettingsCommand
            .Discard()
            .Merge(this.MainContentViewModel.SavePreferencesCommand.Discard())
            .InvokeCommand(this.ServiceViewModel.ReloadSettingsCommand);

        this.MainContentViewModel.SavePreferencesCommand.Subscribe(this.preferencesSaved);

        this.OpenAboutTabCommand.InvokeCommand(this.MainContentViewModel.OpenAboutTabCommand);

        this.PreferencesSaved = this.preferencesSaved.AsObservable();
    }

    public MainContentViewModel MainContentViewModel { get; }
    public ServiceViewModel ServiceViewModel { get; }

    public IObservable<PreferencesModel> PreferencesSaved { get; }

    private CharMappingModel CreateCharMappingModel(AppSettings appSettings, IReadOnlyList<KeyboardLayout> layouts)
    {
        var charsByLayoutId = appSettings.CharsByKeyboardLayoutId;

        var layoutModels = layouts
            .Select((layout, index) => new LayoutModel
            {
                Id = layout.Id,
                Index = index,
                LanguageName = layout.LanguageName,
                KeyboardName = layout.KeyboardName,
                IsNew = charsByLayoutId.Count != 0 && !charsByLayoutId.ContainsKey(layout.Id),
                Chars = charsByLayoutId.GetValueOrDefault(layout.Id, String.Empty)!
            })
            .ToList();

        var missingLayoutIds = charsByLayoutId.Keys
            .Where(id => !layoutModels.Any(layoutModel => layoutModel.Id == id))
            .ToList();

        return new() { Layouts = layoutModels, RemovableLayoutIds = missingLayoutIds };
    }

    [ReactiveCommand]
    private void OpenExternally()
    { }

    [ReactiveCommand]
    private void OpenAboutTab()
    { }
}

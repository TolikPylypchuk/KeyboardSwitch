using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Settings.Core.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly Subject<PreferencesModel> preferencesSaved = new();

    public MainViewModel(
        AppSettings appSettings,
        ILayoutService? layoutService = null,
        IStartupService? startupService = null)
    {
        layoutService ??= GetRequiredService<ILayoutService>();
        startupService ??= GetRequiredService<IStartupService>();

        this.MainContentViewModel = new MainContentViewModel(
            this.CreateCharMappingModel(appSettings, layoutService.GetKeyboardLayouts()),
            new PreferencesModel(appSettings, startupService.IsStartupConfigured()));

        this.ServiceViewModel = new ServiceViewModel();

        this.OpenExternally = ReactiveCommand.Create(() => { });
        this.OpenAboutTab = ReactiveCommand.Create(() => { });

        this.MainContentViewModel.SaveCharMappingSettings
            .Discard()
            .Merge(this.MainContentViewModel.SavePreferences.Discard())
            .InvokeCommand(this.ServiceViewModel.ReloadSettings);

        this.MainContentViewModel.SavePreferences.Subscribe(this.preferencesSaved);

        this.OpenAboutTab.InvokeCommand(this.MainContentViewModel.OpenAboutTab);

        this.PreferencesSaved = this.preferencesSaved.AsObservable();
    }

    public MainContentViewModel MainContentViewModel { get; }
    public ServiceViewModel ServiceViewModel { get; }

    public ReactiveCommand<Unit, Unit> OpenExternally { get; }
    public ReactiveCommand<Unit, Unit> OpenAboutTab { get; }

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
}

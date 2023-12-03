namespace KeyboardSwitch.Settings.Core.ViewModels;

public class MainViewModel : ReactiveObject
{
    private readonly AppSettings appSettings;
    private readonly ILayoutService layoutService;

    public MainViewModel(
        AppSettings appSettings,
        ILayoutService? layoutService = null,
        IStartupService? startupService = null)
    {
        this.appSettings = appSettings;
        this.layoutService = layoutService ?? GetDefaultService<ILayoutService>();
        startupService ??= GetDefaultService<IStartupService>();

        this.MainContentViewModel = new MainContentViewModel(
            this.CreateCharMappingModel(),
            new PreferencesModel(appSettings, startupService.IsStartupConfigured()));

        this.ServiceViewModel = new ServiceViewModel();

        this.OpenExternally = ReactiveCommand.Create(() => { });
        this.OpenAboutTab = ReactiveCommand.Create(() => { });

        this.MainContentViewModel.SaveCharMappingSettings
            .Discard()
            .Merge(this.MainContentViewModel.SavePreferences.Discard())
            .InvokeCommand(this.ServiceViewModel.ReloadSettings);

        this.OpenAboutTab.InvokeCommand(this.MainContentViewModel.OpenAboutTab);
    }

    public MainContentViewModel MainContentViewModel { get; }
    public ServiceViewModel ServiceViewModel { get; }

    public ReactiveCommand<Unit, Unit> OpenExternally { get; }
    public ReactiveCommand<Unit, Unit> OpenAboutTab { get; }

    private CharMappingModel CreateCharMappingModel()
    {
        var layouts = this.layoutService.GetKeyboardLayouts();

        var charsByLayoutId = this.appSettings.CharsByKeyboardLayoutId;

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

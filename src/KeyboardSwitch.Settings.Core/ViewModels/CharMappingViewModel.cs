namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed class CharMappingViewModel : ReactiveForm<CharMappingModel, CharMappingViewModel>
{
    private readonly ILayoutService layoutService;
    private readonly IAutoConfigurationService autoConfigurationService;

    private readonly SourceCache<LayoutModel, string> layoutsSource = new(layout => layout.Id);
    private readonly SourceList<string> removableLayoutIdsSource = new();

    private readonly ReadOnlyObservableCollection<LayoutViewModel> layouts;

    private readonly ObservableAsPropertyHelper<bool> hasNewLayouts;
    private readonly ObservableAsPropertyHelper<bool> canRemoveLayouts;
    private readonly ObservableAsPropertyHelper<bool> shouldRemoveLayouts;

    public CharMappingViewModel(
        CharMappingModel charMappingModel,
        IObservable<bool> removeLayoutsEnabled,
        ILayoutService? layoutService = null,
        IAutoConfigurationService? autoConfigurationService = null,
        ResourceManager? resourceManager = null,
        IScheduler? scheduler = null)
        : base(resourceManager, scheduler)
    {
        this.CharMappingModel = charMappingModel;

        this.layoutService = layoutService ?? GetRequiredService<ILayoutService>();
        this.autoConfigurationService = autoConfigurationService ?? GetRequiredService<IAutoConfigurationService>();

        this.layoutsSource.Connect()
            .Transform(ch => new LayoutViewModel(ch))
            .Sort(SortExpressionComparer<LayoutViewModel>.Ascending(vm => vm.Index))
            .Bind(out this.layouts)
            .Subscribe();

        var canAutoConfigure = this.Layouts
            .ToObservableChangeSet()
            .AutoRefreshOnObservable(layout => layout.Changed)
            .ToCollection()
            .Select(layouts => layouts.All(layout => String.IsNullOrEmpty(layout.Chars)));

        this.AutoConfigure = ReactiveCommand.Create(this.OnAutoConfigure, canAutoConfigure);
        this.RemoveLayouts = ReactiveCommand.Create(() => { });

        this.hasNewLayouts = this.ConfigureHasNewLayouts();
        this.canRemoveLayouts = this.ConfigureCanRemoveLayouts(removeLayoutsEnabled);
        this.shouldRemoveLayouts = this.ConfigureShouldRemoveLayouts();

        this.CopyProperties();
        this.EnableChangeTracking();
    }

    public CharMappingModel CharMappingModel { get; }

    public ReadOnlyObservableCollection<LayoutViewModel> Layouts => this.layouts;

    public bool HasNewLayouts => this.hasNewLayouts.Value;
    public bool CanRemoveLayouts => this.canRemoveLayouts.Value;
    public bool ShouldRemoveLayouts => this.shouldRemoveLayouts.Value;

    public ReactiveCommand<Unit, Unit> AutoConfigure { get; }
    public ReactiveCommand<Unit, Unit> RemoveLayouts { get; }

    protected override CharMappingViewModel Self => this;

    protected override void EnableChangeTracking()
    {
        this.TrackChanges(this.IsCollectionChanged(vm => vm.Layouts, vm => vm.CharMappingModel.Layouts));
        this.TrackChanges(this.WhenAnyValue(vm => vm.HasNewLayouts));
        this.TrackChanges(this.WhenAnyValue(vm => vm.ShouldRemoveLayouts));

        base.EnableChangeTracking();
    }

    protected override async Task<CharMappingModel> OnSaveAsync()
    {
        foreach (var layout in this.Layouts)
        {
            await layout.Save.Execute();
        }

        this.CharMappingModel.Layouts.Clear();
        this.CharMappingModel.Layouts.AddRange(this.layoutsSource.Items);
        this.CharMappingModel.ShouldRemoveLayouts = this.ShouldRemoveLayouts;

        if (this.ShouldRemoveLayouts)
        {
            this.removableLayoutIdsSource.Clear();
        }

        return this.CharMappingModel;
    }

    protected override void CopyProperties()
    {
        this.layoutsSource.Edit(list =>
        {
            list.Clear();
            list.AddOrUpdate(this.CharMappingModel.Layouts);
        });

        this.removableLayoutIdsSource.Edit(list =>
        {
            list.Clear();
            list.AddRange(this.CharMappingModel.RemovableLayoutIds);
        });
    }

    private ObservableAsPropertyHelper<bool> ConfigureHasNewLayouts() =>
        this.Layouts.ToObservableChangeSet()
            .AutoRefresh()
            .ToCollection()
            .Select(layouts => layouts.Any(layout => layout.IsNew))
            .Merge(this.Save.Select(_ => false))
            .ToProperty(this, vm => vm.HasNewLayouts, initialValue: false);

    private ObservableAsPropertyHelper<bool> ConfigureCanRemoveLayouts(IObservable<bool> removeLayoutsEnabled) =>
        this.removableLayoutIdsSource.Connect()
            .Count()
            .Select(count => count > 0)
            .Merge(this.RemoveLayouts.Select(_ => false))
            .CombineLatest(removeLayoutsEnabled, (a, b) => a && b)
            .ToProperty(this, vm => vm.CanRemoveLayouts, initialValue: false);

    private ObservableAsPropertyHelper<bool> ConfigureShouldRemoveLayouts() =>
        this.RemoveLayouts.Select(_ => true)
            .Merge(this.Save.Select(_ => false))
            .Merge(this.Cancel.Select(_ => false))
            .ToProperty(this, vm => vm.ShouldRemoveLayouts, initialValue: false);

    private void OnAutoConfigure()
    {
        var layouts = this.layoutService.GetKeyboardLayouts();
        var charsByLayoutId = this.autoConfigurationService.CreateCharMappings(layouts);

        foreach (var layoutAndChars in charsByLayoutId)
        {
            var layoutViewModel = this.Layouts.First(layout => layout.Id == layoutAndChars.Key);
            layoutViewModel.Chars = layoutAndChars.Value;
        }
    }
}

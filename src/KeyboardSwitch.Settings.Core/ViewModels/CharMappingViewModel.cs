namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed class CharMappingViewModel : ReactiveForm<CharMappingModel, CharMappingViewModel>
{
    private readonly ILayoutService layoutService;
    private readonly IAutoConfigurationService autoConfigurationService;

    private readonly SourceCache<LayoutModel, string> layoutsSource = new(layout => layout.Id);
    private readonly SourceList<string> removableLayoutIdsSource = new();

    private readonly ReadOnlyObservableCollection<LayoutViewModel> layouts;

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

        this.ConfigureLayoutProperties(removeLayoutsEnabled);
        this.CopyProperties();
        this.EnableChangeTracking();
    }

    public CharMappingModel CharMappingModel { get; }

    public ReadOnlyObservableCollection<LayoutViewModel> Layouts => this.layouts;

    public bool HasNewLayouts { [ObservableAsProperty] get; }
    public bool CanRemoveLayouts { [ObservableAsProperty] get; }
    public bool ShouldRemoveLayouts { [ObservableAsProperty] get; }

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

    private void ConfigureLayoutProperties(IObservable<bool> removeLayoutsEnabled)
    {
        this.Layouts.ToObservableChangeSet()
            .AutoRefresh()
            .ToCollection()
            .Select(layouts => layouts.Any(layout => layout.IsNew))
            .Merge(this.Save.Select(_ => false))
            .ToPropertyEx(this, vm => vm.HasNewLayouts, initialValue: false);

        this.removableLayoutIdsSource.Connect()
            .Count()
            .Select(count => count > 0)
            .Merge(this.RemoveLayouts.Select(_ => false))
            .CombineLatest(removeLayoutsEnabled, (a, b) => a && b)
            .ToPropertyEx(this, vm => vm.CanRemoveLayouts, initialValue: false);

        this.RemoveLayouts.Select(_ => true)
            .Merge(this.Save.Select(_ => false))
            .Merge(this.Cancel.Select(_ => false))
            .ToPropertyEx(this, vm => vm.ShouldRemoveLayouts, initialValue: false);
    }

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

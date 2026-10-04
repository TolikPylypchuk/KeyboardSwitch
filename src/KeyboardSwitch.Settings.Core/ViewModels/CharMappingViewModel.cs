namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed partial class CharMappingViewModel : ReactiveForm<CharMappingModel, CharMappingViewModel>
{
    private readonly ILayoutService layoutService;
    private readonly IAutoConfigurationService autoConfigurationService;

    private readonly SourceCache<LayoutModel, string> layoutsSource = new(layout => layout.Id);
    private readonly SourceList<string> removableLayoutIdsSource = new();

    private readonly ReadOnlyObservableCollection<LayoutViewModel> layouts;

    private readonly IObservable<bool> canAutoConfigure;

    public CharMappingViewModel(
        CharMappingModel charMappingModel,
        IObservable<bool> removeLayoutsEnabled,
        ILayoutService? layoutService = null,
        IAutoConfigurationService? autoConfigurationService = null,
        ResourceManager? resourceManager = null)
        : base(resourceManager)
    {
        this.CharMappingModel = charMappingModel;

        this.layoutService = layoutService ?? AppLocator.Current.GetRequiredService<ILayoutService>();
        this.autoConfigurationService = autoConfigurationService
            ?? AppLocator.Current.GetRequiredService<IAutoConfigurationService>();

        this.layoutsSource.Connect()
            .Transform(ch => new LayoutViewModel(ch))
            .SortAndBind(out this.layouts, SortExpressionComparer<LayoutViewModel>.Ascending(vm => vm.Index))
            .Subscribe();

        this.canAutoConfigure = this.Layouts
            .ToObservableChangeSet()
            .AutoRefreshOnObservable(layout => layout.Changed)
            .ToCollection()
            .Select(layouts => layouts.All(layout => String.IsNullOrEmpty(layout.Chars)));

        this._hasNewLayoutsHelper = this.ConfigureHasNewLayouts();
        this._canRemoveLayoutsHelper = this.ConfigureCanRemoveLayouts(removeLayoutsEnabled);
        this._shouldRemoveLayoutsHelper = this.ConfigureShouldRemoveLayouts();

        this.CopyProperties();
        this.EnableChangeTracking();
    }

    [ObservableAsProperty]
    public partial bool HasNewLayouts { get; }

    [ObservableAsProperty]
    public partial bool CanRemoveLayouts { get; }

    [ObservableAsProperty]
    public partial bool ShouldRemoveLayouts { get; }

    public CharMappingModel CharMappingModel { get; }

    public ReadOnlyObservableCollection<LayoutViewModel> Layouts => this.layouts;

    protected override CharMappingViewModel Self => this;

    protected override void EnableChangeTracking()
    {
        this.TrackChanges(this.IsCollectionChanged(vm => vm.Layouts, vm => vm.CharMappingModel.Layouts));
        this.TrackChanges(this.WhenAnyValue(vm => vm.HasNewLayouts));
        this.TrackChanges(this.WhenAnyValue(vm => vm.ShouldRemoveLayouts));

        base.EnableChangeTracking();
    }

    protected override async Task<CharMappingModel> Save()
    {
        foreach (var layout in this.Layouts)
        {
            await layout.SaveCommand.Execute();
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

    [ReactiveCommand(CanExecute = nameof(canAutoConfigure))]
    private async Task AutoConfigure()
    {
        var layouts = await this.layoutService.GetKeyboardLayouts();
        var charsByLayoutId = this.autoConfigurationService.CreateCharMappings(layouts);

        foreach (var layoutAndChars in charsByLayoutId)
        {
            var layoutViewModel = this.Layouts.First(layout => layout.Id == layoutAndChars.Key);
            layoutViewModel.Chars = layoutAndChars.Value;
        }
    }

    [ReactiveCommand]
    private void RemoveLayouts()
    { }

    private ObservableAsPropertyHelper<bool> ConfigureHasNewLayouts() =>
        this.Layouts.ToObservableChangeSet()
            .AutoRefresh()
            .ToCollection()
            .Select(layouts => layouts.Any(layout => layout.IsNew))
            .Merge(this.SaveCommand.Select(_ => false))
            .ToProperty(this, vm => vm.HasNewLayouts, initialValue: false);

    private ObservableAsPropertyHelper<bool> ConfigureCanRemoveLayouts(IObservable<bool> removeLayoutsEnabled) =>
        this.removableLayoutIdsSource.Connect()
            .Count()
            .Select(count => count > 0)
            .Merge(this.RemoveLayoutsCommand.Select(_ => false))
            .CombineLatest(removeLayoutsEnabled, (a, b) => a && b)
            .ToProperty(this, vm => vm.CanRemoveLayouts, initialValue: false);

    private ObservableAsPropertyHelper<bool> ConfigureShouldRemoveLayouts() =>
        this.RemoveLayoutsCommand.Select(_ => true)
            .Merge(this.SaveCommand.Select(_ => false))
            .Merge(this.CancelCommand.Select(_ => false))
            .ToProperty(this, vm => vm.ShouldRemoveLayouts, initialValue: false);
}

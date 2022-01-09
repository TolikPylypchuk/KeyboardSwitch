namespace KeyboardSwitch.Settings.Core.ViewModels;

using SharpHook.Native;

public sealed class PreferencesViewModel : ReactiveForm<PreferencesModel, PreferencesViewModel>
{
    private readonly SourceList<ModifierMask> forwardModifierKeysSource = new();
    private readonly SourceList<ModifierMask> backwardModifierKeysSource = new();

    private readonly ReadOnlyObservableCollection<ModifierMask> forwardModifierKeys;
    private readonly ReadOnlyObservableCollection<ModifierMask> backwardModifierKeys;

    public PreferencesViewModel(
        PreferencesModel preferencesModel,
        ResourceManager? resourceManager = null,
        IScheduler? scheduler = null)
        : base(resourceManager, scheduler)
    {
        this.PreferencesModel = preferencesModel;
        this.CopyProperties();

        this.forwardModifierKeysSource.Connect()
            .Bind(out this.forwardModifierKeys)
            .Subscribe();

        this.backwardModifierKeysSource.Connect()
            .Bind(out this.backwardModifierKeys)
            .Subscribe();

        this.BindKeys();

        this.LocalizedValidationRule(vm => vm.PressCount, count => count > 0 && count <= 10);
        this.LocalizedValidationRule(vm => vm.WaitMilliseconds, wait => wait >= 100 && wait <= 1000);

        this.ModifierKeysAreDifferentRule = this.InitModifierKeysAreDifferentRule();
        this.SwitchMethodsAreDifferentRule = this.InitSwitchMethodsAreDifferentRule();

        this.EnableChangeTracking();
    }

    public PreferencesModel PreferencesModel { get; }

    [Reactive]
    public bool InstantSwitching { get; set; }

    [Reactive]
    public bool SwitchLayout { get; set; }

    [Reactive]
    public bool Startup { get; set; }

    [Reactive]
    public bool ShowUninstalledLayoutsMessage { get; set; }

    [Reactive]
    public bool ShowConverter { get; set; }

    [Reactive]
    public ModifierMask ForwardModifierFirst { get; set; }

    [Reactive]
    public ModifierMask ForwardModifierSecond { get; set; }

    [Reactive]
    public ModifierMask ForwardModifierThird { get; set; }

    [Reactive]
    public ModifierMask BackwardModifierFirst { get; set; }

    [Reactive]
    public ModifierMask BackwardModifierSecond { get; set; }

    [Reactive]
    public ModifierMask BackwardModifierThird { get; set; }

    [Reactive]
    public int PressCount { get; set; }

    [Reactive]
    public int WaitMilliseconds { get; set; }

    public ValidationHelper ModifierKeysAreDifferentRule { get; }
    public ValidationHelper SwitchMethodsAreDifferentRule { get; }

    protected override PreferencesViewModel Self => this;

    protected override void EnableChangeTracking()
    {
        this.TrackChanges(vm => vm.InstantSwitching, vm => vm.PreferencesModel.InstantSwitching);
        this.TrackChanges(vm => vm.SwitchLayout, vm => vm.PreferencesModel.SwitchLayout);
        this.TrackChanges(vm => vm.Startup, vm => vm.PreferencesModel.Startup);

        this.TrackChanges(
            vm => vm.ShowUninstalledLayoutsMessage, vm => vm.PreferencesModel.ShowUninstalledLayoutsMessage);

        this.TrackChanges(vm => vm.ShowConverter, vm => vm.PreferencesModel.ShowConverter);

        this.TrackChanges(this.IsCollectionChangedSimple(
            vm => vm.forwardModifierKeys, vm => vm.PreferencesModel.SwitchSettings.ForwardModifiers));

        this.TrackChanges(this.IsCollectionChangedSimple(
            vm => vm.backwardModifierKeys, vm => vm.PreferencesModel.SwitchSettings.BackwardModifiers));

        this.TrackChanges(vm => vm.PressCount, vm => vm.PreferencesModel.SwitchSettings.PressCount);
        this.TrackChanges(vm => vm.WaitMilliseconds, vm => vm.PreferencesModel.SwitchSettings.WaitMilliseconds);

        base.EnableChangeTracking();
    }

    protected override Task<PreferencesModel> OnSaveAsync()
    {
        this.PreferencesModel.InstantSwitching = this.InstantSwitching;
        this.PreferencesModel.SwitchLayout = this.SwitchLayout;
        this.PreferencesModel.Startup = this.Startup;
        this.PreferencesModel.ShowUninstalledLayoutsMessage = this.ShowUninstalledLayoutsMessage;
        this.PreferencesModel.ShowConverter = this.ShowConverter;

        var switchSettings = this.PreferencesModel.SwitchSettings;

        switchSettings.ForwardModifiers = new(this.forwardModifierKeys);
        switchSettings.BackwardModifiers = new(this.backwardModifierKeys);
        switchSettings.PressCount = this.PressCount;
        switchSettings.WaitMilliseconds = this.WaitMilliseconds;

        return Task.FromResult(this.PreferencesModel);
    }

    protected override void CopyProperties()
    {
        this.InstantSwitching = this.PreferencesModel.InstantSwitching;
        this.SwitchLayout = this.PreferencesModel.SwitchLayout;
        this.Startup = this.PreferencesModel.Startup;
        this.ShowUninstalledLayoutsMessage = this.PreferencesModel.ShowUninstalledLayoutsMessage;
        this.ShowConverter = this.PreferencesModel.ShowConverter;

        var switchSettings = this.PreferencesModel.SwitchSettings;

        if (switchSettings.ForwardModifiers.Count > 0)
        {
            this.ForwardModifierFirst = switchSettings.ForwardModifiers[0];
        }

        if (switchSettings.ForwardModifiers.Count > 1)
        {
            this.ForwardModifierSecond = switchSettings.ForwardModifiers[1];
        }

        if (switchSettings.ForwardModifiers.Count > 2)
        {
            this.ForwardModifierThird = switchSettings.ForwardModifiers[2];
        }

        if (switchSettings.BackwardModifiers.Count > 0)
        {
            this.BackwardModifierFirst = switchSettings.BackwardModifiers[0];
        }

        if (switchSettings.BackwardModifiers.Count > 1)
        {
            this.BackwardModifierSecond = switchSettings.BackwardModifiers[1];
        }

        if (switchSettings.BackwardModifiers.Count > 2)
        {
            this.BackwardModifierThird = switchSettings.BackwardModifiers[2];
        }

        this.PressCount = switchSettings.PressCount;
        this.WaitMilliseconds = switchSettings.WaitMilliseconds;
    }

    private void BindKeys()
    {
        this.WhenAnyValue(
            vm => vm.ForwardModifierFirst,
            vm => vm.ForwardModifierSecond,
            vm => vm.ForwardModifierThird)
            .Subscribe(keys =>
                this.forwardModifierKeysSource.Edit(list =>
                {
                    list.Clear();
                    list.Add(keys.Item1);
                    list.Add(keys.Item2);
                    list.Add(keys.Item3);
                }));

        this.WhenAnyValue(
            vm => vm.BackwardModifierFirst,
            vm => vm.BackwardModifierSecond,
            vm => vm.BackwardModifierThird)
            .Subscribe(keys =>
                this.backwardModifierKeysSource.Edit(list =>
                {
                    list.Clear();
                    list.Add(keys.Item1);
                    list.Add(keys.Item2);
                    list.Add(keys.Item3);
                }));
    }

    private ValidationHelper InitModifierKeysAreDifferentRule()
    {
        var modifierKeysAreDifferent = Observable.CombineLatest(
            this.forwardModifierKeysSource
                .Connect()
                .ToCollection()
                .Select(this.ContainsDistinctElements),
            this.backwardModifierKeysSource
                .Connect()
                .ToCollection()
                .Select(this.ContainsDistinctElements),
            (forward, backward) => forward && backward);

        return this.LocalizedValidationRule(modifierKeysAreDifferent, "ModifierKeysAreSame");
    }

    private ValidationHelper InitSwitchMethodsAreDifferentRule()
    {
        var switchMethodsAreDifferent = Observable.CombineLatest(
            this.forwardModifierKeysSource.Connect().ToCollection(),
            this.backwardModifierKeysSource.Connect().ToCollection(),
            (forward, backward) => !new HashSet<ModifierMask>(forward).SetEquals(backward));

        return this.LocalizedValidationRule(switchMethodsAreDifferent, "SwitchMethodsAreSame");
    }

    private bool ContainsDistinctElements(IReadOnlyCollection<ModifierMask> keys) =>
        !keys
            .SelectMany((x, index1) =>
                keys.Select((y, index2) => (Key1: x, Key2: y, Index1: index1, Index2: index2)))
            .Where(keys => keys.Index1 < keys.Index2)
            .Select(keys => (keys.Key1 & keys.Key2) == ModifierMask.None)
            .Any(equals => !equals);
}

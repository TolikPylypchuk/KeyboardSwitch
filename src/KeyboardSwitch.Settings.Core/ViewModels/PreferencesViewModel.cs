using SharpHook.Data;

namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed class PreferencesViewModel : ReactiveForm<PreferencesModel, PreferencesViewModel>
{
    private bool instantSwitching;
    private bool switchLayout;
    private bool startup;
    private bool showUninstalledLayoutsMessage;
    private bool useXsel;
    private EventMask forwardModifierFirst;
    private EventMask forwardModifierSecond;
    private EventMask forwardModifierThird;
    private EventMask backwardModifierFirst;
    private EventMask backwardModifierSecond;
    private EventMask backwardModifierThird;
    private int pressCount;
    private int waitMilliseconds;
    private AppTheme appTheme;
    private AppThemeVariant appThemeVariant;

    private readonly SourceList<EventMask> forwardModifierKeysSource = new();
    private readonly SourceList<EventMask> backwardModifierKeysSource = new();

    private readonly ReadOnlyObservableCollection<EventMask> forwardModifierKeys;
    private readonly ReadOnlyObservableCollection<EventMask> backwardModifierKeys;

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

    public bool InstantSwitching
    {
        get => this.instantSwitching;
        set => this.RaiseAndSetIfChanged(ref this.instantSwitching, value);
    }

    public bool SwitchLayout
    {
        get => this.switchLayout;
        set => this.RaiseAndSetIfChanged(ref this.switchLayout, value);
    }

    public bool Startup
    {
        get => this.startup;
        set => this.RaiseAndSetIfChanged(ref this.startup, value);
    }

    public bool ShowUninstalledLayoutsMessage
    {
        get => this.showUninstalledLayoutsMessage;
        set => this.RaiseAndSetIfChanged(ref this.showUninstalledLayoutsMessage, value);
    }

    public bool UseXsel
    {
        get => this.useXsel;
        set => this.RaiseAndSetIfChanged(ref this.useXsel, value);
    }

    public EventMask ForwardModifierFirst
    {
        get => this.forwardModifierFirst;
        set => this.RaiseAndSetIfChanged(ref this.forwardModifierFirst, value);
    }

    public EventMask ForwardModifierSecond
    {
        get => this.forwardModifierSecond;
        set => this.RaiseAndSetIfChanged(ref this.forwardModifierSecond, value);
    }

    public EventMask ForwardModifierThird
    {
        get => this.forwardModifierThird;
        set => this.RaiseAndSetIfChanged(ref this.forwardModifierThird, value);
    }

    public EventMask BackwardModifierFirst
    {
        get => this.backwardModifierFirst;
        set => this.RaiseAndSetIfChanged(ref this.backwardModifierFirst, value);
    }

    public EventMask BackwardModifierSecond
    {
        get => this.backwardModifierSecond;
        set => this.RaiseAndSetIfChanged(ref this.backwardModifierSecond, value);
    }

    public EventMask BackwardModifierThird
    {
        get => this.backwardModifierThird;
        set => this.RaiseAndSetIfChanged(ref this.backwardModifierThird, value);
    }

    public int PressCount
    {
        get => this.pressCount;
        set => this.RaiseAndSetIfChanged(ref this.pressCount, value);
    }

    public int WaitMilliseconds
    {
        get => this.waitMilliseconds;
        set => this.RaiseAndSetIfChanged(ref this.waitMilliseconds, value);
    }

    public AppTheme AppTheme
    {
        get => this.appTheme;
        set => this.RaiseAndSetIfChanged(ref this.appTheme, value);
    }

    public AppThemeVariant AppThemeVariant
    {
        get => this.appThemeVariant;
        set => this.RaiseAndSetIfChanged(ref this.appThemeVariant, value);
    }

    public ValidationHelper ModifierKeysAreDifferentRule { get; }
    public ValidationHelper SwitchMethodsAreDifferentRule { get; }

    public bool ShowUseXsel => OperatingSystem.IsLinux();

    protected override PreferencesViewModel Self => this;

    protected override void EnableChangeTracking()
    {
        this.TrackChanges(vm => vm.InstantSwitching, vm => vm.PreferencesModel.InstantSwitching);
        this.TrackChanges(vm => vm.SwitchLayout, vm => vm.PreferencesModel.SwitchLayout);
        this.TrackChanges(vm => vm.Startup, vm => vm.PreferencesModel.Startup);

        this.TrackChanges(
            vm => vm.ShowUninstalledLayoutsMessage, vm => vm.PreferencesModel.ShowUninstalledLayoutsMessage);

        this.TrackChanges(vm => vm.UseXsel, vm => vm.PreferencesModel.UseXsel);

        this.TrackChanges(this.IsCollectionChangedSimple(
            vm => vm.forwardModifierKeys, vm => vm.PreferencesModel.SwitchSettings.ForwardModifiers));

        this.TrackChanges(this.IsCollectionChangedSimple(
            vm => vm.backwardModifierKeys, vm => vm.PreferencesModel.SwitchSettings.BackwardModifiers));

        this.TrackChanges(vm => vm.PressCount, vm => vm.PreferencesModel.SwitchSettings.PressCount);
        this.TrackChanges(vm => vm.WaitMilliseconds, vm => vm.PreferencesModel.SwitchSettings.WaitMilliseconds);

        this.TrackChanges(vm => vm.AppTheme, vm => vm.PreferencesModel.AppTheme);
        this.TrackChanges(vm => vm.AppThemeVariant, vm => vm.PreferencesModel.AppThemeVariant);

        base.EnableChangeTracking();
    }

    protected override Task<PreferencesModel> OnSaveAsync()
    {
        this.PreferencesModel.InstantSwitching = this.InstantSwitching;
        this.PreferencesModel.SwitchLayout = this.SwitchLayout;
        this.PreferencesModel.Startup = this.Startup;
        this.PreferencesModel.ShowUninstalledLayoutsMessage = this.ShowUninstalledLayoutsMessage;
        this.PreferencesModel.UseXsel = this.UseXsel;
        this.PreferencesModel.AppTheme = this.AppTheme;
        this.PreferencesModel.AppThemeVariant = this.AppThemeVariant;

        this.PreferencesModel.SwitchSettings = this.PreferencesModel.SwitchSettings with
        {
            ForwardModifiers = [..this.forwardModifierKeys],
            BackwardModifiers = [..this.backwardModifierKeys],
            PressCount = this.PressCount,
            WaitMilliseconds = this.WaitMilliseconds,
        };

        return Task.FromResult(this.PreferencesModel);
    }

    protected override void CopyProperties()
    {
        this.InstantSwitching = this.PreferencesModel.InstantSwitching;
        this.SwitchLayout = this.PreferencesModel.SwitchLayout;
        this.Startup = this.PreferencesModel.Startup;
        this.ShowUninstalledLayoutsMessage = this.PreferencesModel.ShowUninstalledLayoutsMessage;
        this.UseXsel = this.PreferencesModel.UseXsel;
        this.AppTheme = this.PreferencesModel.AppTheme;
        this.AppThemeVariant = this.PreferencesModel.AppThemeVariant;

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
            (forward, backward) => !new HashSet<EventMask>(forward).SetEquals(backward));

        return this.LocalizedValidationRule(switchMethodsAreDifferent, "SwitchMethodsAreSame");
    }

    private bool ContainsDistinctElements(IReadOnlyCollection<EventMask> keys) =>
        !keys
            .SelectMany((x, index1) =>
                keys.Select((y, index2) => (Key1: x, Key2: y, Index1: index1, Index2: index2)))
            .Where(keys => keys.Index1 < keys.Index2)
            .Select(keys => (keys.Key1 & keys.Key2) == EventMask.None)
            .Any(equals => !equals);
}

namespace KeyboardSwitch.Settings.Core.Services;

public class ConverterAppSettingsService : ReactiveObject, IAppSettingsService
{
    private readonly Subject<Unit> settingsInvalidated = new();

    public ConverterAppSettingsService(
        IObservable<CustomLayoutModel> sourceLayout,
        IObservable<CustomLayoutModel> targetLayout)
    {
        sourceLayout.ToPropertyEx(this, vm => vm.SourceLayout);
        targetLayout.ToPropertyEx(this, vm => vm.TargetLayout);
    }

    private CustomLayoutModel SourceLayout { [ObservableAsProperty] get; } = null!;
    private CustomLayoutModel TargetLayout { [ObservableAsProperty] get; } = null!;

    public IObservable<Unit> SettingsInvalidated =>
        this.settingsInvalidated.AsObservable();

    public Task<AppSettings> GetAppSettingsAsync() =>
        Task.FromResult(new AppSettings
        {
            CharsByKeyboardLayoutId = new()
            {
                [SourceLayoutId] = this.SourceLayout.Chars,
                [TargetLayoutId] = this.TargetLayout.Chars
            },
            InstantSwitching = false,
            SwitchLayout = false
        });

    public void InvalidateAppSettings() =>
        this.settingsInvalidated.OnNext(Unit.Default);

    public Task SaveAppSettingsAsync(AppSettings appSettings) =>
        Task.CompletedTask;
}

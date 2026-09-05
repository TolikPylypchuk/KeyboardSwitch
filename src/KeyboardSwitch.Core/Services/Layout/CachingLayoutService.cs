namespace KeyboardSwitch.Core.Services.Layout;

public abstract class CachingLayoutService : ILayoutService
{
    private Task<List<KeyboardLayout>>? systemLayouts;
    private readonly Subject<Unit> settingsInvalidated = new();

    public CachingLayoutService() =>
        this.settingsInvalidated.Subscribe(_ =>
        {
            this.systemLayouts = null;
            this.OnSettingsInvalidated();
        });

    public IObserver<Unit> SettingsInvalidated =>
        this.settingsInvalidated.AsObserver();

    public abstract Task<KeyboardLayout> GetCurrentKeyboardLayout();

    public abstract Task SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings);

    public async Task<IReadOnlyList<KeyboardLayout>> GetKeyboardLayouts()
    {
        // The task itself is cached, so that concurrent callers share a single lookup
        var layouts = this.systemLayouts ??= this.GetKeyboardLayoutsInternal();

        try
        {
            return (await layouts).AsReadOnly();
        } catch
        {
            if (this.systemLayouts == layouts)
            {
                this.systemLayouts = null;
            }

            throw;
        }
    }

    protected abstract Task<List<KeyboardLayout>> GetKeyboardLayoutsInternal();

    protected virtual void OnSettingsInvalidated()
    { }
}

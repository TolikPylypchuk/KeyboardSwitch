namespace KeyboardSwitch.Core.Services.Layout;

public abstract class CachingLayoutService : ILayoutService
{
    private List<KeyboardLayout>? systemLayouts;
    private readonly Subject<Unit> settingsInvalidated = new();

    public CachingLayoutService() =>
        this.settingsInvalidated.Subscribe(_ => this.systemLayouts = null);

    public IObserver<Unit> SettingsInvalidated =>
        this.settingsInvalidated.AsObserver();

    public abstract KeyboardLayout GetCurrentKeyboardLayout();

    public abstract void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings);

    public IReadOnlyList<KeyboardLayout> GetKeyboardLayouts()
    {
        if (this.systemLayouts != null)
        {
            return this.systemLayouts.AsReadOnly();
        }

        this.systemLayouts = this.GetKeyboardLayoutsInternal();

        return this.systemLayouts.AsReadOnly();
    }

    protected abstract List<KeyboardLayout> GetKeyboardLayoutsInternal();
}

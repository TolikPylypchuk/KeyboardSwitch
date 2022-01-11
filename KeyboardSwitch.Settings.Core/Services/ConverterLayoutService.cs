namespace KeyboardSwitch.Settings.Core.Services;

public class ConverterLayoutService : ReactiveObject, ILayoutService
{
    public ConverterLayoutService(
        IObservable<CustomLayoutModel> sourceLayout,
        IObservable<CustomLayoutModel> targetLayout)
    {
        sourceLayout
            .Select(layout => this.CreateFakeKeyboardLayout(layout, SourceLayoutId))
            .ToPropertyEx(this, vm => vm.SourceLayout);

        targetLayout
            .Select(layout => this.CreateFakeKeyboardLayout(layout, TargetLayoutId))
            .ToPropertyEx(this, vm => vm.TargetLayout);
    }

    private KeyboardLayout SourceLayout { [ObservableAsProperty] get; } = null!;
    private KeyboardLayout TargetLayout { [ObservableAsProperty] get; } = null!;

    public KeyboardLayout GetCurrentKeyboardLayout() =>
        this.SourceLayout;

    public IReadOnlyList<KeyboardLayout> GetKeyboardLayouts() =>
        new List<KeyboardLayout>() { this.SourceLayout, this.TargetLayout }.AsReadOnly();

    private KeyboardLayout CreateFakeKeyboardLayout(CustomLayoutModel layout, string index) =>
        new(index, CultureInfo.InvariantCulture.EnglishName, layout.Name, index);

    IObserver<Unit> ILayoutService.SettingsInvalidated { get; } = Observer.Create<Unit>(unit => { });

    void ILayoutService.SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    { }
}

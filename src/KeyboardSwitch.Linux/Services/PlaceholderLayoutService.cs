using System.Reactive;

namespace KeyboardSwitch.Linux.Services;

internal sealed class PlaceholderLayoutService : ILayoutService
{
    private readonly KeyboardLayout placeholderLayout = new("us:", "us", "English (US)", "us");

    public IObserver<Unit> SettingsInvalidated { get; } = Observer.Create<Unit>(_ => { });

    public KeyboardLayout GetCurrentKeyboardLayout() =>
        placeholderLayout;

    public IReadOnlyList<KeyboardLayout> GetKeyboardLayouts() =>
        [placeholderLayout];

    public void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    { }
}

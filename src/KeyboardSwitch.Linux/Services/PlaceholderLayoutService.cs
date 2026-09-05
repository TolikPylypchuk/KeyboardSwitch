using System.Reactive;

namespace KeyboardSwitch.Linux.Services;

internal sealed class PlaceholderLayoutService : ILayoutService
{
    private readonly KeyboardLayout placeholderLayout = new("us:", "us", "English (US)", "us");

    public IObserver<Unit> SettingsInvalidated { get; } = Observer.Create<Unit>(_ => { });

    public Task<KeyboardLayout> GetCurrentKeyboardLayout() =>
        Task.FromResult(placeholderLayout);

    public Task<IReadOnlyList<KeyboardLayout>> GetKeyboardLayouts() =>
        Task.FromResult<IReadOnlyList<KeyboardLayout>>([placeholderLayout]);

    public Task SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings) =>
        Task.CompletedTask;
}

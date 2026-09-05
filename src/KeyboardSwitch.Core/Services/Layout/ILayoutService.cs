namespace KeyboardSwitch.Core.Services.Layout;

public interface ILayoutService
{
    IObserver<Unit> SettingsInvalidated { get; }

    Task<KeyboardLayout> GetCurrentKeyboardLayout();
    Task SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings);
    Task<IReadOnlyList<KeyboardLayout>> GetKeyboardLayouts();
}

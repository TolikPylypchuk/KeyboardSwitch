namespace KeyboardSwitch.Core.Services.Layout;

public interface ILayoutService
{
    IObserver<Unit> SettingsInvalidated { get; }

    KeyboardLayout GetCurrentKeyboardLayout();
    void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings);
    IReadOnlyList<KeyboardLayout> GetKeyboardLayouts();
}

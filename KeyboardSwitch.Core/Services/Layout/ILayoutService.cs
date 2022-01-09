namespace KeyboardSwitch.Core.Services.Layout;

public interface ILayoutService
{
    KeyboardLayout GetCurrentKeyboardLayout();
    void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings);
    List<KeyboardLayout> GetKeyboardLayouts();
}

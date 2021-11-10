namespace KeyboardSwitch.Core.Services.Layout;

public interface ILayoutService
{
    bool SwitchLayoutsViaKeyboardSimulation { get; }

    KeyboardLayout GetCurrentKeyboardLayout();
    void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings);
    List<KeyboardLayout> GetKeyboardLayouts();
}

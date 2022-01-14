namespace KeyboardSwitch.MacOS.Services;

internal class MacLayoutService : CachingLayoutService
{
    public override KeyboardLayout GetCurrentKeyboardLayout() =>
        new(String.Empty, String.Empty, String.Empty, String.Empty);

    public override void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    { }

    protected override List<KeyboardLayout> GetKeyboardLayoutsInternal() =>
        new();
}

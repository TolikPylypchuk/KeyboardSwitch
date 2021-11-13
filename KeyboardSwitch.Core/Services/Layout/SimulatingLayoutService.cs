namespace KeyboardSwitch.Core.Services.Layout;

public abstract class SimulatingLayoutService : ILayoutService
{
    private readonly ILogger logger;

    public SimulatingLayoutService(ILogger logger) =>
        this.logger = logger;

    public bool SwitchLayoutsViaKeyboardSimulation => true;

    public abstract KeyboardLayout GetCurrentKeyboardLayout();
    public abstract List<KeyboardLayout> GetKeyboardLayouts();

    public void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
    {
        this.logger.LogDebug("Switching the current layout {Direction} via keyboard simulation", direction.AsString());

        var keys = direction switch
        {
            SwitchDirection.Forward => settings.LayoutForwardKeys,
            SwitchDirection.Backward => settings.LayoutBackwardKeys,
            _ => throw new InvalidOperationException($"Invalid switch direction: {direction}")
        };

        this.SimulateKeyPresses(keys);
    }

    protected abstract void SimulateKeyPresses(IEnumerable<KeyCode> keys);
}

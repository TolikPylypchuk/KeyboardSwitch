namespace KeyboardSwitch.Core.Services.Layout;

public abstract class SimulatingLayoutService : ILayoutService
{
    private readonly IEventSimulator eventSimulator;
    private readonly ILogger logger;

    public SimulatingLayoutService(IEventSimulator eventSimulator, ILogger logger)
    {
        this.eventSimulator = eventSimulator;
        this.logger = logger;
    }

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

        foreach (var key in keys)
        {
            eventSimulator.SimulateKeyPress(key);
        }

        foreach (var key in Enumerable.Reverse(keys))
        {
            eventSimulator.SimulateKeyRelease(key);
        }
    }
}

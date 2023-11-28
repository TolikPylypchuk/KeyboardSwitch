namespace KeyboardSwitch.Core.Services.Simulation;

public class SharpUserActivitySimulator(IEventSimulator eventSimulator, KeyCode modifierKey) : IUserActivitySimulator
{
    private const int Delay = 16;

    private readonly IEventSimulator eventSimulator = eventSimulator;
    private readonly KeyCode modifierKey = modifierKey;

    public async Task SimulateCopy()
    {
        this.eventSimulator.SimulateKeyPress(this.modifierKey);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyPress(KeyCode.VcC);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyRelease(KeyCode.VcC);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyRelease(this.modifierKey);
        await Task.Delay(Delay);
    }

    public async Task SimulatePaste()
    {
        this.eventSimulator.SimulateKeyPress(this.modifierKey);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyPress(KeyCode.VcV);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyRelease(KeyCode.VcV);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyRelease(this.modifierKey);
        await Task.Delay(Delay);
    }
}

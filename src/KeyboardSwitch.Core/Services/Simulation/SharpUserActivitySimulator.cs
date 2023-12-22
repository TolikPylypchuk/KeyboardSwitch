namespace KeyboardSwitch.Core.Services.Simulation;

public class SharpUserActivitySimulator(IEventSimulator eventSimulator, KeyCode modifierKey) : IUserActivitySimulator
{
    private const int Delay = 16;

    public async Task SimulateCopy()
    {
        eventSimulator.SimulateKeyPress(modifierKey);
        await Task.Delay(Delay);

        eventSimulator.SimulateKeyPress(KeyCode.VcC);
        await Task.Delay(Delay);

        eventSimulator.SimulateKeyRelease(KeyCode.VcC);
        await Task.Delay(Delay);

        eventSimulator.SimulateKeyRelease(modifierKey);
        await Task.Delay(Delay);
    }

    public async Task SimulatePaste()
    {
        eventSimulator.SimulateKeyPress(modifierKey);
        await Task.Delay(Delay);

        eventSimulator.SimulateKeyPress(KeyCode.VcV);
        await Task.Delay(Delay);

        eventSimulator.SimulateKeyRelease(KeyCode.VcV);
        await Task.Delay(Delay);

        eventSimulator.SimulateKeyRelease(modifierKey);
        await Task.Delay(Delay);
    }
}

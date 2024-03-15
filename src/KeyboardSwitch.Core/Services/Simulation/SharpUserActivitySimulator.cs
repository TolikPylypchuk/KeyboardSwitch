namespace KeyboardSwitch.Core.Services.Simulation;

public class SharpUserActivitySimulator(
    IEventSimulator eventSimulator,
    IScheduler scheduler,
    KeyCode modifierKey) : IUserActivitySimulator
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(16);

    public async Task SimulateCopy()
    {
        eventSimulator.SimulateKeyPress(modifierKey);
        await scheduler.Sleep(Delay);

        eventSimulator.SimulateKeyPress(KeyCode.VcC);
        await scheduler.Sleep(Delay);

        eventSimulator.SimulateKeyRelease(KeyCode.VcC);
        await scheduler.Sleep(Delay);

        eventSimulator.SimulateKeyRelease(modifierKey);
        await scheduler.Sleep(Delay);
    }

    public async Task SimulatePaste()
    {
        eventSimulator.SimulateKeyPress(modifierKey);
        await scheduler.Sleep(Delay);

        eventSimulator.SimulateKeyPress(KeyCode.VcV);
        await scheduler.Sleep(Delay);

        eventSimulator.SimulateKeyRelease(KeyCode.VcV);
        await scheduler.Sleep(Delay);

        eventSimulator.SimulateKeyRelease(modifierKey);
        await scheduler.Sleep(Delay);
    }
}

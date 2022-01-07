namespace KeyboardSwitch.Core.Services.Simulation;

internal class UserActivitySimulator : IUserActivitySimulator
{
    private const int Delay = 16;

    private readonly IEventSimulator eventSimulator;

    public UserActivitySimulator(IEventSimulator eventSimulator) =>
        this.eventSimulator = eventSimulator;

    public async Task SimulateCopyAsync()
    {
        this.eventSimulator.SimulateKeyPress(KeyCode.VcLeftControl);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyPress(KeyCode.VcC);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyRelease(KeyCode.VcC);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyRelease(KeyCode.VcLeftControl);
        await Task.Delay(Delay);
    }

    public async Task SimulatePasteAsync()
    {
        this.eventSimulator.SimulateKeyPress(KeyCode.VcLeftControl);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyPress(KeyCode.VcV);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyRelease(KeyCode.VcV);
        await Task.Delay(Delay);

        this.eventSimulator.SimulateKeyRelease(KeyCode.VcLeftControl);
        await Task.Delay(Delay);
    }
}

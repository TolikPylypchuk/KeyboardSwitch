namespace KeyboardSwitch.Core.Services.Simulation;

public class SharpUserActivitySimulator(
    IEventSimulator eventSimulator,
    IScheduler scheduler,
    KeyCode modifierKey) : IUserActivitySimulator
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(16);

    private readonly IEventSimulationSequenceTemplate copyTemplate = eventSimulator.Sequence()
        .AddKeyStroke(modifierKey, KeyCode.VcC)
        .CreateTemplate();

    private readonly IEventSimulationSequenceTemplate pasteTemplate = eventSimulator.Sequence()
        .AddKeyStroke(modifierKey, KeyCode.VcV)
        .CreateTemplate();

    public async Task SimulateCopy()
    {
        this.copyTemplate.Simulate();
        await scheduler.Sleep(Delay);
    }

    public async Task SimulatePaste()
    {
        this.pasteTemplate.Simulate();
        await scheduler.Sleep(Delay);
    }
}

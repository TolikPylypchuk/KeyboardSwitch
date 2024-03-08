namespace KeyboardSwitch.Core.Services.Simulation;

public class SharpUserActivitySimulator(
    IEventSimulator eventSimulator,
    IScheduler scheduler,
    KeyCode modifierKey) : IUserActivitySimulator
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(16);
    private static readonly IObservable<Unit> DummyObservable = Observable.Return(Unit.Default);

    public async Task SimulateCopy()
    {
        eventSimulator.SimulateKeyPress(modifierKey);
        await DummyObservable.Delay(Delay, scheduler);

        eventSimulator.SimulateKeyPress(KeyCode.VcC);
        await DummyObservable.Delay(Delay, scheduler);

        eventSimulator.SimulateKeyRelease(KeyCode.VcC);
        await DummyObservable.Delay(Delay, scheduler);

        eventSimulator.SimulateKeyRelease(modifierKey);
        await DummyObservable.Delay(Delay, scheduler);
    }

    public async Task SimulatePaste()
    {
        eventSimulator.SimulateKeyPress(modifierKey);
        await DummyObservable.Delay(Delay, scheduler);

        eventSimulator.SimulateKeyPress(KeyCode.VcV);
        await DummyObservable.Delay(Delay, scheduler);

        eventSimulator.SimulateKeyRelease(KeyCode.VcV);
        await DummyObservable.Delay(Delay, scheduler);

        eventSimulator.SimulateKeyRelease(modifierKey);
        await DummyObservable.Delay(Delay, scheduler);
    }
}

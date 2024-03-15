namespace KeyboardSwitch.Tests.Services;

using KeyboardSwitch.Core.Services.Simulation;

using SharpHook.Testing;

public sealed class SharpUserActivitySimulatorTests
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(16);

    [Theory(DisplayName = "User activity simulator should simulate copying")]
    [InlineData(KeyCode.VcLeftControl)]
    [InlineData(KeyCode.VcRightControl)]
    [InlineData(KeyCode.VcLeftMeta)]
    [InlineData(KeyCode.VcRightMeta)]
    public void SimulateCopy(KeyCode modifier)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();

        var simulator = new SharpUserActivitySimulator(hook, scheduler, modifier);

        // Act

        var task = simulator.SimulateCopy();

        while (!task.IsCompleted)
        {
            scheduler.AdvanceBy(Delay.Ticks);
        }

        // Assert

        Assert.Equal(4, hook.SimulatedEvents.Count);

        var firstPress = hook.SimulatedEvents[0];
        var secondPress = hook.SimulatedEvents[1];
        var firstRelease = hook.SimulatedEvents[2];
        var secondRelease = hook.SimulatedEvents[3];

        Assert.Equal(EventType.KeyPressed, firstPress.Type);
        Assert.Equal(modifier, firstPress.Keyboard.KeyCode);

        Assert.Equal(EventType.KeyPressed, secondPress.Type);
        Assert.Equal(KeyCode.VcC, secondPress.Keyboard.KeyCode);

        Assert.Equal(EventType.KeyReleased, firstRelease.Type);
        Assert.Equal(KeyCode.VcC, firstRelease.Keyboard.KeyCode);

        Assert.Equal(EventType.KeyReleased, secondRelease.Type);
        Assert.Equal(modifier, secondRelease.Keyboard.KeyCode);
    }

    [Theory(DisplayName = "User activity simulator should simulate pasting")]
    [InlineData(KeyCode.VcLeftControl)]
    [InlineData(KeyCode.VcRightControl)]
    [InlineData(KeyCode.VcLeftMeta)]
    [InlineData(KeyCode.VcRightMeta)]
    public void SimulatePaste(KeyCode modifier)
    {
        // Arrange

        using var hook = new TestGlobalHook();
        var scheduler = new TestScheduler();

        var simulator = new SharpUserActivitySimulator(hook, scheduler, modifier);

        // Act

        var task = simulator.SimulatePaste();

        while (!task.IsCompleted)
        {
            scheduler.AdvanceBy(Delay.Ticks);
        }

        // Assert

        Assert.Equal(4, hook.SimulatedEvents.Count);

        var firstPress = hook.SimulatedEvents[0];
        var secondPress = hook.SimulatedEvents[1];
        var firstRelease = hook.SimulatedEvents[2];
        var secondRelease = hook.SimulatedEvents[3];

        Assert.Equal(EventType.KeyPressed, firstPress.Type);
        Assert.Equal(modifier, firstPress.Keyboard.KeyCode);

        Assert.Equal(EventType.KeyPressed, secondPress.Type);
        Assert.Equal(KeyCode.VcV, secondPress.Keyboard.KeyCode);

        Assert.Equal(EventType.KeyReleased, firstRelease.Type);
        Assert.Equal(KeyCode.VcV, firstRelease.Keyboard.KeyCode);

        Assert.Equal(EventType.KeyReleased, secondRelease.Type);
        Assert.Equal(modifier, secondRelease.Keyboard.KeyCode);
    }
}

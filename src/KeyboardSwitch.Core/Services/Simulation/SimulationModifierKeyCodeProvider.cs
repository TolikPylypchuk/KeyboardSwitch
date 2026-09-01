namespace KeyboardSwitch.Core.Services.Simulation;

public sealed record class SimulationModifierKeyCodeProvider
{
    private SimulationModifierKeyCodeProvider(KeyCode keyCode) =>
        this.KeyCode = keyCode;

    public static SimulationModifierKeyCodeProvider Control { get; } = new(KeyCode.VcLeftControl);

    public static SimulationModifierKeyCodeProvider Command { get; } = new(KeyCode.VcLeftMeta);

    public KeyCode KeyCode { get; }

    public override string ToString() =>
        this.KeyCode.ToString();
}

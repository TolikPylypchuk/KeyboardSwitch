namespace KeyboardSwitch.Settings.State;

public sealed class AppState
{
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }

    public bool IsWindowMaximized { get; set; }

    public bool IsInitialized { get; set; }
}

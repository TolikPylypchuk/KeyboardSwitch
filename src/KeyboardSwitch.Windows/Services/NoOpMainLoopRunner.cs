namespace KeyboardSwitch.Windows.Services;

internal sealed class NoOpMainLoopRunner : IMainLoopRunner
{
    public void RunMainLoop(CancellationToken token)
    { }
}

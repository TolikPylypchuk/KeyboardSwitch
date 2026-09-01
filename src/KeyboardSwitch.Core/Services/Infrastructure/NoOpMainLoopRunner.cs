namespace KeyboardSwitch.Core.Services.Infrastructure;

public sealed class NoOpMainLoopRunner : IMainLoopRunner
{
    public void RunMainLoop(CancellationToken token)
    { }
}

namespace KeyboardSwitch.Core.Services.Infrastructure;

public class NoOpMainLoopRunner : IMainLoopRunner
{
    public void RunMainLoopIfNeeded(CancellationToken token)
    { }
}

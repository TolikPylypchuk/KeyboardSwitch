namespace KeyboardSwitch.Core.Services.Infrastructure;

public class NoOpMainLoopRunner : IMainLoopRunner
{
    public void RunMainLoopIfNeeded()
    { }

    public void Dispose()
    { }
}

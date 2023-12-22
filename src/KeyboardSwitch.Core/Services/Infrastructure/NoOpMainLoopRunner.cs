namespace KeyboardSwitch.Core.Services.Infrastructure;

public class NoOpMainLoopRunner : IMainLoopRunner
{
    public bool ShouldRunMainLoop => false;

    public void RunMainLoop()
    { }

    public void Dispose()
    { }
}

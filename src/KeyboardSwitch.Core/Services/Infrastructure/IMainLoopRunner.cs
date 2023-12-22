namespace KeyboardSwitch.Core.Services.Infrastructure;

public interface IMainLoopRunner : IDisposable
{
    public bool ShouldRunMainLoop { get; }

    public void RunMainLoop();
}

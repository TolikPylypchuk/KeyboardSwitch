namespace KeyboardSwitch.Core.Services.Infrastructure;

public interface IMainLoopRunner : IDisposable
{
    public void RunMainLoopIfNeeded();
}

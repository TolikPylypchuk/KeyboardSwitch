namespace KeyboardSwitch.Core.Services.Infrastructure;

public interface IMainLoopRunner
{
    public void RunMainLoop(CancellationToken token);
}

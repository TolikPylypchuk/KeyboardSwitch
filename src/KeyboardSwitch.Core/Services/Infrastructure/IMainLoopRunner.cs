namespace KeyboardSwitch.Core.Services.Infrastructure;

public interface IMainLoopRunner
{
    public void RunMainLoopIfNeeded(CancellationToken token);
}

namespace KeyboardSwitch.Services;

public interface IExitService
{
    ExitCode ExitCode { get; }

    Task Exit(ExitCode exitCode, CancellationToken token = default);
}

namespace KeyboardSwitch.Services;

public sealed class ExitService(IHost host) : IExitService
{
    public ExitCode ExitCode { get; private set; } = ExitCode.Success;

    public async Task Exit(ExitCode exitCode, CancellationToken token = default)
    {
        this.ExitCode = exitCode;
        await host.StopAsync(token);
    }
}

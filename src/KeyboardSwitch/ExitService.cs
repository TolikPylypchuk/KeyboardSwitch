namespace KeyboardSwitch;

public sealed class ExitService(IHost host, ILogger<ExitService> logger) : IExitService
{
    public ExitCode ExitCode { get; private set; } = ExitCode.Success;

    public async Task Exit(ExitCode exitCode, CancellationToken token = default)
    {
        logger.LogDebug(
            "Initiating stopping the KeyboardSwitch service with exit code {ExitCodeValue}: {ExitCode}",
            (int)exitCode,
            exitCode);

        this.ExitCode = exitCode;
        await host.StopAsync(token);
    }
}

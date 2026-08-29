namespace KeyboardSwitch;

public sealed partial class ExitService(IHost host, ILogger<ExitService> logger) : IExitService
{
    public ExitCode ExitCode { get; private set; } = ExitCode.Success;

    public async Task Exit(ExitCode exitCode, CancellationToken token = default)
    {
        this.LogInitiatingStoppingService((int)exitCode, exitCode);

        this.ExitCode = exitCode;
        await host.StopAsync(token);
    }

    [LoggerMessage(
        LogLevel.Debug, "Initiating stopping the KeyboardSwitch service with exit code {ExitCodeValue}: {ExitCode}")]
    private partial void LogInitiatingStoppingService(int exitCodeValue, ExitCode exitCode);
}

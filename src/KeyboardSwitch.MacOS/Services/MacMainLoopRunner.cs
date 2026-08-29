namespace KeyboardSwitch.MacOS.Services;

internal sealed partial class MacMainLoopRunner(ILogger<MacMainLoopRunner> logger) : IMainLoopRunner
{
    public void RunMainLoop(CancellationToken token)
    {
        this.LogRunningMainRunLoop();

        var loop = CoreFoundation.CFRunLoopGetCurrent();
        token.Register(() => CoreFoundation.CFRunLoopStop(loop));
        CoreFoundation.CFRunLoopRun();
    }

    [LoggerMessage(LogLevel.Information, "Running the main run-loop")]
    private partial void LogRunningMainRunLoop();
}

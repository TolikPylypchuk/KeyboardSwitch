namespace KeyboardSwitch.MacOS.Services;

internal sealed class MacMainLoopRunner(ILogger<MacMainLoopRunner> logger) : IMainLoopRunner
{
    public void RunMainLoop(CancellationToken token)
    {
        logger.LogInformation("Running the main run-loop");

        var loop = CoreFoundation.CFRunLoopGetCurrent();
        token.Register(() => CoreFoundation.CFRunLoopStop(loop));
        CoreFoundation.CFRunLoopRun();
    }
}

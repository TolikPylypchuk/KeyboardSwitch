namespace KeyboardSwitch.MacOS.Services;

internal sealed class MacMainLoopRunner : IMainLoopRunner
{
    public void RunMainLoop(CancellationToken token)
    {
        var loop = CoreFoundation.CFRunLoopGetCurrent();
        token.Register(() => CoreFoundation.CFRunLoopStop(loop));
        CoreFoundation.CFRunLoopRun();
    }
}

namespace KeyboardSwitch.MacOS.Services;

internal sealed class MacOSMainLoopRunner : IMainLoopRunner
{
    public void RunMainLoop(CancellationToken token)
    {
        var loop = CoreFoundation.CFRunLoopGetCurrent();
        token.Register(() => CoreFoundation.CFRunLoopStop(loop));
        CoreFoundation.CFRunLoopRun();
    }
}

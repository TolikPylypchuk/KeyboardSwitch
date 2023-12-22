namespace KeyboardSwitch.MacOS.Services;

internal sealed class MacOSMainLoopRunner : Disposable, IMainLoopRunner
{
    private CFRunLoopRef? loop;

    public bool ShouldRunMainLoop => true;

    ~MacOSMainLoopRunner() =>
        this.Dispose(false);

    public void RunMainLoop()
    {
        this.ThrowIfDisposed();

        this.loop = CoreFoundation.CFRunLoopGetCurrent();
        CoreFoundation.CFRunLoopRun();
    }

    protected override void Dispose(bool disposing)
    {
        if (this.loop is not null)
        {
            CoreFoundation.CFRunLoopStop(this.loop);
            this.loop = null;
        }
    }
}

namespace KeyboardSwitch.MacOS.Services;

internal sealed class MacOSMainLoopRunner : Disposable, IMainLoopRunner
{
    private CFRunLoopRef? loop;

    ~MacOSMainLoopRunner() =>
        this.Dispose(false);

    public void RunMainLoopIfNeeded()
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

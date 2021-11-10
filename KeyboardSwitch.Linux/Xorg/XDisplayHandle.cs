namespace KeyboardSwitch.Linux.Xorg;

internal sealed class XDisplayHandle : XHandleBase
{
    public XDisplayHandle(IntPtr handle) =>
        this.SetHandle(handle);

    public XDisplayHandle()
    { }

    protected override bool ReleaseHandle() =>
        Xlib.XCloseDisplay(this.handle) != Status.Failure;
}

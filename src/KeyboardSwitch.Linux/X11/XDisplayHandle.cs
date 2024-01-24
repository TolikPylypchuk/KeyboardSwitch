namespace KeyboardSwitch.Linux.X11;

internal sealed class XDisplayHandle : XHandleBase
{
    public XDisplayHandle()
    { }

    public XDisplayHandle(IntPtr handle) =>
        this.SetHandle(handle);

    protected override bool ReleaseHandle() =>
        XLib.XCloseDisplay(this.handle) != XStatus.Failure;
}

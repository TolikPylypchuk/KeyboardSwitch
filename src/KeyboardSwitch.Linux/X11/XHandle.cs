namespace KeyboardSwitch.Linux.X11;

internal sealed class XHandle : XHandleBase
{
    protected override bool ReleaseHandle() =>
        XLib.XFree(this.handle) != XStatus.Failure;
}

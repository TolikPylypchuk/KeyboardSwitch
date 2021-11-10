namespace KeyboardSwitch.Linux.Xorg;

internal sealed class XHandle : XHandleBase
{
    protected override bool ReleaseHandle() =>
        XFree(this.handle);
}

namespace KeyboardSwitch.Linux.X11;

internal class XMapHandle : XHandleBase
{
    private readonly XkbMapComponentMask componentMask;

    public XMapHandle()
    { }

    public XMapHandle(IntPtr handle, XkbMapComponentMask componentMask)
        : base(handle) =>
        this.componentMask = componentMask;

    public override bool IsInvalid =>
        this.handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        XLib.XkbFreeClientMap(this.DangerousGetHandle(), this.componentMask, true);
        return true;
    }
}

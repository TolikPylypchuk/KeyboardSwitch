namespace KeyboardSwitch.Linux.Xkb;

internal sealed class XkbContextHandle : SafeHandle
{
    public XkbContextHandle()
        : base(IntPtr.Zero, true)
    { }

    public override bool IsInvalid =>
        this.handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        XkbCommon.ContextUnref(this.handle);
        return true;
    }
}

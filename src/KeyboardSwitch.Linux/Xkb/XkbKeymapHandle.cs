namespace KeyboardSwitch.Linux.Xkb;

internal sealed class XkbKeymapHandle : SafeHandle
{
    public XkbKeymapHandle()
        : base(IntPtr.Zero, true)
    { }

    public override bool IsInvalid =>
        this.handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        XkbCommon.KeymapUnref(this.handle);
        return true;
    }
}

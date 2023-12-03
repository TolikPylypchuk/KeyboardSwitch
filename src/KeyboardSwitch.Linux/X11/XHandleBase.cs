namespace KeyboardSwitch.Linux.X11;

internal abstract class XHandleBase : SafeHandle
{
    private protected XHandleBase()
        : base(IntPtr.Zero, true)
    { }

    private protected XHandleBase(IntPtr ptr)
        : base(ptr, true)
    { }

    public override bool IsInvalid =>
        this.handle == IntPtr.Zero;
}

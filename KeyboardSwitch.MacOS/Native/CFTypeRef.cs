namespace KeyboardSwitch.MacOS.Native;

internal abstract class CFTypeRef : SafeHandle
{
    private protected CFTypeRef()
        : base(IntPtr.Zero, true)
    { }

    private protected CFTypeRef(IntPtr ptr)
        : base(ptr, true)
    { }

    public override bool IsInvalid =>
        this.handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        CoreFoundation.CFRelease(this.handle);
        return true;
    }
}

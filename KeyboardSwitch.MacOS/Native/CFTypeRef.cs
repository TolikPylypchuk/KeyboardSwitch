namespace KeyboardSwitch.MacOS.Native;

internal abstract class CFTypeRef : SafeHandle
{
    private protected CFTypeRef()
        : base(IntPtr.Zero, true)
    { }

    private protected CFTypeRef(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    { }

    private protected CFTypeRef(IntPtr ptr)
        : base(ptr, true)
    { }

    private protected CFTypeRef(IntPtr ptr, bool ownsHandle)
        : base(ptr, ownsHandle)
    { }

    public override bool IsInvalid =>
        this.handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        if (!this.IsInvalid)
        {
            CoreFoundation.CFRelease(this.handle);
        }

        return true;
    }
}

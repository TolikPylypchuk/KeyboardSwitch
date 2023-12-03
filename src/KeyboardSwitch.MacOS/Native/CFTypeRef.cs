namespace KeyboardSwitch.MacOS.Native;

internal abstract class CFTypeRef : SafeHandle
{
    public CFTypeRef()
        : base(IntPtr.Zero, true)
    { }

    public CFTypeRef(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    { }

    public CFTypeRef(IntPtr ptr)
        : base(ptr, true)
    { }

    public CFTypeRef(IntPtr ptr, bool ownsHandle)
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

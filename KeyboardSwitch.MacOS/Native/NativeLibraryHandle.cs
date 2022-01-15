namespace KeyboardSwitch.MacOS.Native;

internal class NativeLibraryHandle : SafeHandle
{
    public NativeLibraryHandle(IntPtr handle)
        : base(handle, true)
    { }

    public override bool IsInvalid =>
        this.handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeLibrary.Free(this.handle);
        return true;
    }
}

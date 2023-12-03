namespace KeyboardSwitch.MacOS.Native;

internal class NativeLibraryHandle(IntPtr handle) : SafeHandle(handle, true)
{
    public override bool IsInvalid =>
        this.handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeLibrary.Free(this.handle);
        return true;
    }
}

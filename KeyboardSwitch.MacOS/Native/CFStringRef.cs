namespace KeyboardSwitch.MacOS.Native;

internal class CFStringRef : CFTypeRef
{
    public CFStringRef()
        : base(IntPtr.Zero, false)
    { }

    public CFStringRef(IntPtr ptr)
        : base(ptr, false)
    { }
}

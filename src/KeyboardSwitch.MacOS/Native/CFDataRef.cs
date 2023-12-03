namespace KeyboardSwitch.MacOS.Native;

internal class CFDataRef : CFTypeRef
{
    public CFDataRef()
        : base(IntPtr.Zero, false)
    { }

    public CFDataRef(IntPtr ptr)
        : base(ptr, false)
    { }
}

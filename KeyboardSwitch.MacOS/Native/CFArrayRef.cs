namespace KeyboardSwitch.MacOS.Native;

internal class CFArrayRef : CFTypeRef
{
    public CFArrayRef()
        : base(IntPtr.Zero)
    { }

    public CFArrayRef(IntPtr ptr)
        : base(ptr)
    { }
}

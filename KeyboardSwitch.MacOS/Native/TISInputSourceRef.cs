namespace KeyboardSwitch.MacOS.Native;

internal class TISInputSourceRef : CFTypeRef
{
    public TISInputSourceRef()
        : base(IntPtr.Zero, false)
    { }

    public TISInputSourceRef(IntPtr ptr)
        : base(ptr, false)
    { }
}

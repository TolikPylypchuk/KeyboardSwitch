namespace KeyboardSwitch.MacOS.Native;

internal class CGEventSourceRef : CFTypeRef
{
    public CGEventSourceRef()
        : base(IntPtr.Zero)
    { }

    public CGEventSourceRef(IntPtr ptr)
        : base(ptr)
    { }
}

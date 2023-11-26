namespace KeyboardSwitch.MacOS.Native;

internal class CGEventRef : CFTypeRef
{
    public CGEventRef()
        : base(IntPtr.Zero)
    { }

    public CGEventRef(IntPtr ptr)
        : base(ptr)
    { }
}

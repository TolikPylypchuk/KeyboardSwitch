namespace KeyboardSwitch.MacOS.Native;

internal class CGEventRef : CFTypeRef
{
    private protected CGEventRef()
        : base(IntPtr.Zero)
    { }

    private protected CGEventRef(IntPtr ptr)
        : base(ptr)
    { }
}

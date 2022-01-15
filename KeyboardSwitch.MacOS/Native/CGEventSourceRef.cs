namespace KeyboardSwitch.MacOS.Native;

internal class CGEventSourceRef : CFTypeRef
{
    private protected CGEventSourceRef()
        : base(IntPtr.Zero)
    { }

    private protected CGEventSourceRef(IntPtr ptr)
        : base(ptr)
    { }
}

namespace KeyboardSwitch.MacOS.Native;

internal class CFArrayRef : CFTypeRef
{
    private protected CFArrayRef()
        : base(IntPtr.Zero, true)
    { }

    private protected CFArrayRef(IntPtr ptr)
        : base(ptr, true)
    { }
}

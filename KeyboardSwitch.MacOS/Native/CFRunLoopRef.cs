namespace KeyboardSwitch.MacOS.Native;

internal sealed class CFRunLoopRef : CFTypeRef
{
    public CFRunLoopRef()
        : base(IntPtr.Zero, false)
    { }

    public CFRunLoopRef(IntPtr ptr)
        : base(ptr, false)
    { }
}

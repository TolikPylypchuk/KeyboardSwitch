namespace KeyboardSwitch.MacOS.Native;

internal class CFDictionaryRef : CFTypeRef
{
    public CFDictionaryRef()
        : base(IntPtr.Zero, true)
    { }

    public CFDictionaryRef(IntPtr ptr)
        : base(ptr, true)
    { }
}

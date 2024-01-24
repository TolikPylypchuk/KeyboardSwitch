namespace KeyboardSwitch.Linux.Native;

[StructLayout(LayoutKind.Explicit)]
internal struct EPollData
{
    [FieldOffset(0)]
    public IntPtr Ptr;

    [FieldOffset(0)]
    public int Fd;

    [FieldOffset(0)]
    public uint U32;

    [FieldOffset(0)]
    public ulong U64;
}

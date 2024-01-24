namespace KeyboardSwitch.Linux.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct EPollEvent
{
    public uint Events;
    public EPollData Data;
}

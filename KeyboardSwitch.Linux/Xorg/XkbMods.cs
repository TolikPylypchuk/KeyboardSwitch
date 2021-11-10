namespace KeyboardSwitch.Linux.Xorg;

[StructLayout(LayoutKind.Sequential)]
internal struct XkbMods
{
    public byte Mask;
    public byte RealMods;
    public ushort VMods;
}

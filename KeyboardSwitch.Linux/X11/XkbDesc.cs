namespace KeyboardSwitch.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal struct XkbDesc
{
    public IntPtr Display;

    public ushort Flags;
    public ushort DeviceSpec;

    public byte MinKeyCode;
    public byte MaxKeyCode;

    public IntPtr Ctrls;
    public IntPtr Server;
    public IntPtr Map;
    public IntPtr Indicators;
    public IntPtr Names;
    public IntPtr Compat;
    public IntPtr Geom;
}

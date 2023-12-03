namespace KeyboardSwitch.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal struct XkbNames
{
    public Atom KeyCodes;
    public Atom Geometry;
    public Atom Symbols;
    public Atom Types;
    public Atom Compat;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = XkbNumVirtualMods)]
    public Atom[] VMods;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = XkbNumIndicators)]
    public Atom[] Indicators;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = XkbNumKbdGroups)]
    public Atom[] Groups;
}

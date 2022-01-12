namespace KeyboardSwitch.Linux.X11;

[StructLayout(LayoutKind.Sequential)]
internal struct XkbState
{
    public byte Group;
    public byte LockedGroup;
    public ushort BaseGroup;
    public ushort LatchedGroup;
    public byte Mods;
    public byte BaseMods;
    public byte LatchedMods;
    public byte LockedMods;
    public byte CompatState;
    public byte GrabMods;
    public byte CompatGrabMods;
    public byte LookupMods;
    public byte CompatLookupMods;
    public ushort PtrButtons;
}

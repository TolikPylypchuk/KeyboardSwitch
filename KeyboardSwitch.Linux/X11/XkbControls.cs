using System.Runtime.InteropServices;

namespace KeyboardSwitch.Linux.X11
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XkbControls
    {
        public byte MkDefaultButton;
        public byte NumGroups;
        public byte GroupsWrap;
        public XkbMods Internal;
        public XkbMods IgnoreLock;
        public uint EnabledCtrls;
        public ushort RepeatDelay;
        public ushort RepeatInterval;
        public ushort SlowKeysDelay;
        public ushort DebounceDelay;
        public ushort MkDelay;
        public ushort MkInternal;
        public ushort MkTimeToMax;
        public ushort mk_max_speed;
        public short MkCurve;
        public ushort AxOptions;
        public ushort AxTimeout;
        public ushort AxtOptsMask;
        public ushort AxtOptsValues;
        public uint AxtCtrlsMask;
        public uint AxtCtrlsValues;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] PerKeyRepeat;
    }
}

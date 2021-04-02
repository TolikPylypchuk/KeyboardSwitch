using System;
using System.Runtime.InteropServices;

namespace KeyboardSwitch.Linux.X11
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct XkbDesc
    {
        public IntPtr Dpy;
        public ushort Flags;
        public ushort DeviceSpec;
        public ushort MinKeyCode;
        public ushort MaxKeyCode;
        public IntPtr Ctrls;
        public IntPtr Server;
        public IntPtr Map;
        public IntPtr Indicators;
        public IntPtr Names;
        public IntPtr Compat;
        public IntPtr Geom;
    }
}

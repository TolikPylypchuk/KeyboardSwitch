using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyboardSwitch.Common.Interop
{
#pragma warning disable IDE1006 // Naming Styles

    internal static class WindowsNativeFunctions
    {
        private const string User32 = "user32.dll";

        [DllImport(User32)]
        internal static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport(User32, CharSet = CharSet.Unicode)]
        internal static extern int GetKeyboardLayoutName(StringBuilder pwszKlid);

        [DllImport(User32, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadKeyboardLayout(string pwszKlid, uint flags);

        [DllImport(User32)]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport(User32)]
        internal static extern bool PostMessage(IntPtr hhwnd, uint msg, IntPtr wparam, IntPtr lparam);

        [DllImport(User32)]
        internal static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport(User32)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        [DllImport(User32)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport(User32)]
        internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }

#pragma warning restore IDE1006 // Naming Styles
}

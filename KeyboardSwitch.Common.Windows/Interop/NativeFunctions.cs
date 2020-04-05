using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

using KeyboardSwitch.Common.Windows.Services;

namespace KeyboardSwitch.Common.Windows.Interop
{
    internal static class NativeFunctions
    {
        public const string User32 = "user32.dll";

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(
            int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport(User32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport(User32, CharSet = CharSet.Unicode, ExactSpelling = false)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(User32)]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport(User32)]
        public static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport(User32)]
        public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);
    }
}

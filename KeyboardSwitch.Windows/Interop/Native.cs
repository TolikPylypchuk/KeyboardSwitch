using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;

using KeyboardSwitch.Windows.Services;

namespace KeyboardSwitch.Windows.Interop
{
    internal static class Native
    {
        public const string User32 = "user32.dll";

        public const int WhKeyboardLL = 13;

        public static readonly IntPtr WmKeyDown = (IntPtr)0x0100;
        public static readonly IntPtr WmKeyUp =(IntPtr)0x0101;
        public static readonly IntPtr WmSysKeyDown = (IntPtr)0x0104;
        public static readonly IntPtr WmSysKeyUp = (IntPtr)0x0105;

        public const int WmInputLangChangeRequest = 0x50;

        public const int HklNext = 1;
        public const int HklPrev = 0;

        public const int KlfActivate = 0x00000001;
        public const int KlNameLength = 9;

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

        [DllImport(User32)]
        public static extern short VkKeyScan(char ch);

        [DllImport(User32)]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport(User32)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport(User32)]
        public static extern int GetKeyboardLayout(int idThread);

        [DllImport(User32)]
        public static extern int GetKeyboardLayoutList(int nBuff, IntPtr[]? lpList);

        [DllImport(User32, CharSet = CharSet.Unicode)]
        public static extern bool GetKeyboardLayoutName([Out] StringBuilder pwszKLID);

        [DllImport(User32)]
        public static extern int ActivateKeyboardLayout(int hkl, uint flags);

        [DllImport(User32, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint flags);

        [DllImport(User32)]
        public static extern bool PostMessage(IntPtr hhwnd, int msg, IntPtr wparam, IntPtr lparam);
    }
}

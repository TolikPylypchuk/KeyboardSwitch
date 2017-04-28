using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KeyboardSwitch
{
	internal class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetKeyboardLayout(uint idThread);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetKeyboardLayoutName(
			StringBuilder pwszKLID);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr LoadKeyboardLayout(
			string pwszKLID,
			uint flags);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool PostMessage(
			IntPtr hhwnd,
			uint msg,
			IntPtr wparam,
			IntPtr lparam);

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(
			IntPtr hWnd,
			IntPtr processId);

		[DllImport("user32.dll")]
		public static extern bool RegisterHotKey(
			IntPtr hWnd,
			int id,
			uint fsModifiers,
			uint vlc);

		[DllImport("user32.dll")]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	}
}

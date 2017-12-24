using System;
using System.Globalization;
using System.Text;
using System.Windows.Input;

using static KeyboardSwitch.Interop.NativeFunctions;

namespace KeyboardSwitch.Services
{
	public class LayoutManager : ILayoutManager
	{
		private LayoutManager() { }

		public static LayoutManager Current { get; } = new LayoutManager();

		public CultureInfo GetCurrentLayout()
		{
			var layout = GetKeyboardLayout(
				GetWindowThreadProcessId(
					GetForegroundWindow(), IntPtr.Zero));

			return new CultureInfo((short)layout.ToInt64());
		}

		public void SetCurrentLayout(CultureInfo value)
		{
			var lang = InputLanguageManager.Current.CurrentInputLanguage;
			InputLanguageManager.Current.CurrentInputLanguage = value;

			var input = new StringBuilder(9);

			GetKeyboardLayoutName(input);

			PostMessage(
				GetForegroundWindow(),
				0x0050,
				IntPtr.Zero,
				LoadKeyboardLayout(input.ToString(), 1));

			InputLanguageManager.Current.CurrentInputLanguage = lang;
		}
	}
}

using System;
using System.Globalization;
using System.Text;
using System.Windows.Input;

using static KeyboardSwitch.Interop.NativeFunctions;

namespace KeyboardSwitch.Services
{
	public class LayoutManager : ILayoutManager
	{
		public CultureInfo GetCurrentLayout()
		{
			long layout = GetKeyboardLayout(
				GetWindowThreadProcessId(
					GetForegroundWindow(), IntPtr.Zero))
				.ToInt64();

			return layout == 0
				? null
				: new CultureInfo((short)layout);
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

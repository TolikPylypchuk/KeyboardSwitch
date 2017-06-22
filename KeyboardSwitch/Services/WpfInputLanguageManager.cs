using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace KeyboardSwitch.Services
{
	public class WpfInputLanguageManager : IInputLanguageManager
	{
		private WpfInputLanguageManager() { }

		public static WpfInputLanguageManager Current { get; } =
			new WpfInputLanguageManager();

		public IEnumerable<CultureInfo> InputLanguages =>
			InputLanguageManager.Current.AvailableInputLanguages
				?.Cast<CultureInfo>()
					?? new List<CultureInfo>();
	}
}

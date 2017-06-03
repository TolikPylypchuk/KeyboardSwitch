using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace KeyboardSwitch.Services
{
	public class DefaultInputLanguageManager : IInputLanguageManager
	{
		private DefaultInputLanguageManager() { }

		public static DefaultInputLanguageManager Current { get; } =
			new DefaultInputLanguageManager();

		public IEnumerable<CultureInfo> InputLanguages =>
			InputLanguageManager.Current.AvailableInputLanguages
				?.Cast<CultureInfo>()
					?? new List<CultureInfo>();
	}
}

using System.Collections.Generic;
using System.Globalization;

namespace KeyboardSwitch.Services
{
	public interface IInputLanguageManager
	{
		IEnumerable<CultureInfo> InputLanguages { get; }
	}
}

using System.Globalization;

namespace KeyboardSwitch.Services
{
	public interface ILayoutManager
	{
		CultureInfo GetCurrentLayout();
		void SetCurrentLayout(CultureInfo value);
	}
}

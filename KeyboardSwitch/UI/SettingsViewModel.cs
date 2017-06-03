using System.Windows;
using System.Windows.Controls;

namespace KeyboardSwitch.UI
{
	public class SettingsViewModel
	{
		public App CurrentApp { get; } = Application.Current as App;
		public bool CanSave { get; set; }
		public bool CanClose { get; set; }
		public bool IsBorderClicked { get; set; }
		public Border FocusedBorder { get; set; }
	}
}

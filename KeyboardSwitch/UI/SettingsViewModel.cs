using System.Windows.Controls;

namespace KeyboardSwitch.UI
{
	public class SettingsViewModel
	{
		public bool CanSave { get; set; }
		public bool CanClose { get; set; }
		public bool IsBorderClicked { get; set; }
		public Border FocusedBorder { get; set; }
	}
}

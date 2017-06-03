using System.Windows;
using System.Windows.Input;

using KeyboardSwitch.Properties;

namespace KeyboardSwitch.UI
{
	public class HotKeyViewModel
	{
		public App CurrentApp { get; } = Application.Current as App;

		public Key KeyForward { get; set; }
		public Key KeyBackward { get; set; }

		public char CharForward { get; set; }
		public char CharBackward { get; set; }
		public int Index { get; set; }

		public HotKeyViewModel()
		{
			this.CharForward = Settings.Default.HotKeyForward;
			this.CharBackward = Settings.Default.HotKeyBackward;

			this.KeyForward = App.GetKey(this.CharForward);
			this.KeyBackward = App.GetKey(this.CharBackward);
		}
	}
}

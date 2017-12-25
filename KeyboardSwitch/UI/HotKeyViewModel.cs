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
		
		public Key InstantKeyForward { get; set; }
		public Key InstantKeyBackward { get; set; }

		public char CharForward { get; set; }
		public char CharBackward { get; set; }

		public char CharInstantForward { get; set; }
		public char CharInstantBackward { get; set; }

		public int IndexDefault	{ get; set; }
		public int IndexInstant { get; set; }

		public HotKeyViewModel()
		{
			this.CharForward = Settings.Default.HotKeyForward;
			this.CharBackward = Settings.Default.HotKeyBackward;

			this.KeyForward = App.GetKey(this.CharForward);
			this.KeyBackward = App.GetKey(this.CharBackward);

			this.CharInstantForward = Settings.Default.HotKeyInstantForward;
			this.CharInstantBackward = Settings.Default.HotKeyInstantBackward;

			this.InstantKeyForward = App.GetKey(this.CharInstantForward);
			this.InstantKeyBackward = App.GetKey(this.CharInstantBackward);
		}
	}
}

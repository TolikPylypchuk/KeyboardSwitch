using System;

namespace KeyboardSwitch.Services
{
	[Flags]
	public enum KeyModifier
	{
		None = 0x0000,
		Alt = 0x0001,
		Ctrl = 0x0002,
		Shift = 0x0004,
		Win = 0x0008,
		NoRepeat = 0x4000
	}
}

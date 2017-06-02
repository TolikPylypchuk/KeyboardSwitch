using System;

using KeyboardSwitch.Infrastructure;

namespace KeyboardSwitch
{
	public class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			var wrapper = new SingleInstanceWrapper();
			wrapper.Run(args);
		}
	}
}

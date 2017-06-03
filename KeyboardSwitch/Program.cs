using System;

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

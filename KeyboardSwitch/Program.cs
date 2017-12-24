using System;

namespace KeyboardSwitch
{
	public class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			new SingleInstanceWrapper().Run(args);
		}
	}
}

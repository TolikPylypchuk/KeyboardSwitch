using Microsoft.VisualBasic.ApplicationServices;

namespace KeyboardSwitch.Infrastructure
{
	internal class SingleInstanceWrapper : WindowsFormsApplicationBase
	{
		private App app;

		public SingleInstanceWrapper()
		{
			this.IsSingleInstance = true;
		}

		protected override bool OnStartup(StartupEventArgs e)
		{
			this.app = new App();
			this.app.Run();

			return false;
		}

		protected override void OnStartupNextInstance(
			StartupNextInstanceEventArgs e)
		{
			this.app.ProcessNextInstance();
		}
	}
}

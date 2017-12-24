using Microsoft.VisualBasic.ApplicationServices;

namespace KeyboardSwitch
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
			DependencyInjector.InjectDependencies();

			this.app = DependencyInjector.GetNewApp();
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

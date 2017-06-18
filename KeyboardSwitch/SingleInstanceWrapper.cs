using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualBasic.ApplicationServices;

using KeyboardSwitch.Services;

namespace KeyboardSwitch
{
	[ExcludeFromCodeCoverage]
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

			this.app = new App(LanguageManager.Current);
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

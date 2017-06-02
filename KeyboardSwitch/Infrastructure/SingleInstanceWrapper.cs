using System.Diagnostics.CodeAnalysis;

using Microsoft.VisualBasic.ApplicationServices;

using KeyboardSwitch.Services;

namespace KeyboardSwitch.Infrastructure
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
			this.app = new App(
				FileManager.Current,
				LanguageManager.Current);
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

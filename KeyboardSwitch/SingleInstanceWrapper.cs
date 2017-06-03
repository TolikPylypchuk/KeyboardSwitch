using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

using Microsoft.VisualBasic.ApplicationServices;

using KeyboardSwitch.Interop;
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
			this.InjectDependencies();

			this.app = new App(FileManager.Current, LanguageManager.Current);
			this.app.Run();

			return false;
		}

		protected override void OnStartupNextInstance(
			StartupNextInstanceEventArgs e)
		{
			this.app.ProcessNextInstance();
		}

		private void InjectDependencies()
		{
			FileManager.Current.MappingsLocation = Path.Combine(
				Path.GetDirectoryName(
					Assembly.GetEntryAssembly().Location)
				?? Environment.CurrentDirectory,
				ConfigurationManager.AppSettings["MappingsLocation"]);

			LanguageManager.Current.InputLanguageManager =
				DefaultInputLanguageManager.Current;
			LanguageManager.Current.LayoutManager = LayoutManager.Current;
			LanguageManager.Current.TextManager = ClipboardTextManager.Current;
		}
	}
}

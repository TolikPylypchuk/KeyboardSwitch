using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

using WindowsInput;

using KeyboardSwitch.Services;

namespace KeyboardSwitch
{
	[ExcludeFromCodeCoverage]
	public static class DependencyInjector
	{
		public static App GetNewApp()
			=> new App(FileManager.Current, LanguageManager.Current);

		public static void InjectDependencies()
		{
			FileManager.Current.MappingsLocation = Path.Combine(
				Path.GetDirectoryName(
					Assembly.GetEntryAssembly().Location)
				?? Environment.CurrentDirectory,
				ConfigurationManager.AppSettings["MappingsLocation"]);

			LanguageManager.Current.InputLanguageManager =
				WpfInputLanguageManager.Current;
			LanguageManager.Current.LayoutManager = LayoutManager.Current;

			InstantClipboardTextManager.Current.Keyboard =
				new InputSimulator().Keyboard;
		}

		public static ITextManager DefaultTextManager
			=> ClipboardTextManager.Current;

		public static ITextManager InstantTextManager
			=> InstantClipboardTextManager.Current;
	}
}

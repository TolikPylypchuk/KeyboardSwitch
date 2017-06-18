﻿using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using KeyboardSwitch.Interop;
using KeyboardSwitch.Services;

namespace KeyboardSwitch
{
	public static class DependencyInjector
	{
		public static void InjectDependencies()
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

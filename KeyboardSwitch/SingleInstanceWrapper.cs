using System;
using System.Configuration;
using System.IO;
using System.Reflection;

using Microsoft.VisualBasic.ApplicationServices;

using Unity;
using Unity.Injection;
using WindowsInput;

using KeyboardSwitch.Services;
using KeyboardSwitch.UI;

namespace KeyboardSwitch
{
	internal class SingleInstanceWrapper : WindowsFormsApplicationBase
	{
		private App app;
		private IUnityContainer container = new UnityContainer();

		public SingleInstanceWrapper()
		{
			this.IsSingleInstance = true;
		}

		protected override bool OnStartup(StartupEventArgs e)
		{
			this.SetUpContainer();

			this.app = this.container.Resolve<App>();
			this.app.Run();

			return false;
		}

		protected override void OnStartupNextInstance(
			StartupNextInstanceEventArgs e)
		{
			this.app.ProcessNextInstance(e.CommandLine);
		}

		private void SetUpContainer()
		{
			const string defaultTextManagerName = "Default";
			const string instantTextManagerName = "Instant";

			this.container.RegisterInstance(this.container);

			string mappingsLocation = Path.Combine(
				Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
				?? Environment.CurrentDirectory,
				ConfigurationManager.AppSettings["MappingsFile"]);
			
			this.container.RegisterSingleton<FileManager>(
				new InjectionConstructor(mappingsLocation));
			
			this.container.RegisterType<IInputSimulator, InputSimulator>(
				new InjectionConstructor());

			this.container.RegisterType<IKeyboardSimulator, KeyboardSimulator>();
			this.container.RegisterType<ILayoutManager, LayoutManager>();
			this.container.RegisterType<IInputLanguageManager, WpfInputLanguageManager>();

			this.container.RegisterType<ITextManager, ClipboardTextManager>(
				defaultTextManagerName);

			this.container.RegisterType<ITextManager, InstantClipboardTextManager>(
				instantTextManagerName);

			this.container.RegisterSingleton<LanguageManager>();

			this.container.RegisterSingleton<App>(
				new InjectionConstructor(
					new ResolvedParameter<IUnityContainer>(),
					new ResolvedParameter<FileManager>(),
					new ResolvedParameter<LanguageManager>(),
					new ResolvedParameter<ITextManager>(defaultTextManagerName),
					new ResolvedParameter<ITextManager>(instantTextManagerName)));

			this.container.RegisterSingleton<SettingsViewModel>(
				new InjectionConstructor(
					new ResolvedParameter<App>(),
					new ResolvedParameter<LanguageManager>(),
					new ResolvedParameter<ITextManager>(defaultTextManagerName)));
		}
	}
}

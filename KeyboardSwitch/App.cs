using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using KeyboardSwitch.Properties;
using KeyboardSwitch.Services;
using KeyboardSwitch.UI;

namespace KeyboardSwitch
{
	[ExcludeFromCodeCoverage]
	public class App : Application
	{
		public App(FileManager fileManager, LanguageManager languageManager)
		{
			this.FileManager = fileManager;
			this.LanguageManager = languageManager;
		}

		public FileManager FileManager { get; private set; }
		public LanguageManager LanguageManager { get; private set; }

		public HotKey HotKeyForward { get; set; }
		public HotKey HotKeyBackward { get; set; }
		
		public static Key GetKey(char value)
		{
			value = Char.ToUpper(value);
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(value))
			{
				return (Key)(value - 'A' + (int)Key.A);
			}

			throw new ArgumentOutOfRangeException(
				nameof(value),
				"Only English letters can be set here.");
		}
		
		public static void BringWindowToForeground(Window window)
		{
			if (window.WindowState == WindowState.Minimized ||
				window.Visibility == Visibility.Hidden)
			{
				window.Show();
				window.WindowState = WindowState.Normal;
			}

			window.Activate();
			window.Topmost = true;
			window.Topmost = false;
			window.Focus();
		}

		public void ProcessNextInstance()
		{
			if (this.MainWindow is SettingsWindow window)
			{
				window.BringToForeground();
			} else
			{
				BringWindowToForeground(MainWindow);
			}
		}
		
		public void HotKeyPressed(HotKey key)
		{
			this.LanguageManager.SwitchText(key == this.HotKeyForward);
		}

		protected override async void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			var langs = FileManager.Read();
			this.LanguageManager.Languages = langs;

			if (langs == null)
			{
				new ErrorWindow(
					null,
					"Cannot read character mappings.\n" +
					"You have to define them yourself.")
					.ShowDialog();
			}
			
			var modifiers = Settings.Default.KeyModifiers;

			this.HotKeyForward = new HotKey(
				GetKey(Settings.Default.HotKeyForward),
				modifiers,
				this.HotKeyPressed);

			this.HotKeyBackward = new HotKey(
				GetKey(Settings.Default.HotKeyBackward),
				modifiers,
				this.HotKeyPressed);
			
			this.MainWindow = new SettingsWindow();
			this.MainWindow.Show();

			if (Array.Exists(
					Environment.GetCommandLineArgs(),
					s => s.Equals("--nowindow")))
			{
				await Task.Delay(500);
				this.MainWindow.Hide();
			}

			await this.HandleLanguageLoopAsync();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.HotKeyForward.Dispose();
			this.HotKeyBackward.Dispose();
			base.OnExit(e);
		}

		[SuppressMessage("ReSharper", "FunctionNeverReturns")]
		private async Task HandleLanguageLoopAsync()
		{
			while (true)
			{
				this.LanguageManager.HandleCurrentLanguage();
				await Task.Delay(100);
			}
		}
	}
}

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
	[SuppressMessage("ReSharper", "UnusedVariable")]
	[SuppressMessage("ReSharper", "FunctionNeverReturns")]
	public class App : Application
	{
		public App(LanguageManager manager)
		{
			this.LanguageManager = manager;
		}

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

			var task = Task.Factory.StartNew(async () =>
			{
				while (true)
				{
					this.LanguageManager.HandleCurrentLanguage();
					await Task.Delay(100);
				}
			});

			bool success = FileManager.TryRead(out var langs);
			this.LanguageManager.Languages = langs;

			if (!success)
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
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.HotKeyForward.Dispose();
			this.HotKeyBackward.Dispose();
			base.OnExit(e);
		}
	}
}

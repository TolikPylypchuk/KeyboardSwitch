﻿using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Unity;

using KeyboardSwitch.Properties;
using KeyboardSwitch.Services;
using KeyboardSwitch.UI;

namespace KeyboardSwitch
{
	public class App : Application
	{
		#region Constructors

		public App(
			IUnityContainer container,
			FileManager fileManager,
			LanguageManager langManager,
			ITextManager defaultTextManager,
			ITextManager instantTextManager)
		{
			this.Container = container;
			this.FileManager = fileManager;
			this.LanguageManager = langManager;
			this.DefaultTextManager = defaultTextManager;
			this.InstantTextManager = instantTextManager;
		}

		#endregion

		#region Properties

		public HotKey HotKeyForward { get; set; }
		public HotKey HotKeyBackward { get; set; }

		public HotKey HotKeyInstantForward { get; set; }
		public HotKey HotKeyInstantBackward { get; set; }

		public IUnityContainer Container { get; }
		public FileManager FileManager { get; }
		public LanguageManager LanguageManager { get; }
		public ITextManager DefaultTextManager { get; }
		public ITextManager InstantTextManager { get; }

		#endregion

		#region Public methods

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
				BringWindowToForeground(this.MainWindow);
			}
		}

		public void HotKeyPressed(HotKey key)
		{
			Debug.WriteLine($"In {nameof(this.HotKeyPressed)}()");

			this.SwitchText(
				key == this.HotKeyForward || key == this.HotKeyInstantForward,
				key == this.HotKeyForward || key == this.HotKeyBackward
					? this.DefaultTextManager
					: this.InstantTextManager);
		}

		public void SwitchText(bool forward, ITextManager textManager)
			=> this.LanguageManager.SwitchText(textManager, forward);

		#endregion

		#region Protected methods

		protected override async void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			this.SetLanguages();
			this.SetHotKeys();
			this.SetJumpList();

			await this.CreateMainWindow();
			await this.HandleLanguageLoopAsync();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			this.HotKeyForward.Dispose();
			this.HotKeyBackward.Dispose();
			base.OnExit(e);
		}

		#endregion

		#region Private methods

		private void SetLanguages()
		{
			this.LanguageManager.Languages = this.FileManager.Read();

			if (this.LanguageManager.Languages == null)
			{
				MessageBox.Show(
					"Cannot read character mappings.\n" +
					"You have to define them yourself.",
					"Keyboard Layout Switch - Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);

				this.LanguageManager.Languages =
					this.LanguageManager.InputLanguageManager.InputLanguages
						.ToDictionary(lang => lang, _ => new StringBuilder(" "));
			}
		}

		private void SetHotKeys()
		{
			var modifiers = Settings.Default.KeyModifiers;

			this.HotKeyForward = new HotKey(
				GetKey(Settings.Default.HotKeyForward),
				modifiers,
				this.HotKeyPressed);

			this.HotKeyBackward = new HotKey(
				GetKey(Settings.Default.HotKeyBackward),
				modifiers,
				this.HotKeyPressed);

			modifiers = Settings.Default.InstantKeyModifiers;

			this.HotKeyInstantForward = new HotKey(
				GetKey(Settings.Default.HotKeyInstantForward),
				modifiers,
				this.HotKeyPressed);

			this.HotKeyInstantBackward = new HotKey(
				GetKey(Settings.Default.HotKeyInstantBackward),
				modifiers,
				this.HotKeyPressed);
		}

		private void SetJumpList()
		{

		}

		private async Task CreateMainWindow()
		{
			this.MainWindow = this.Container.Resolve<SettingsWindow>();
			this.MainWindow.Show();

			if (Array.Exists(
				Environment.GetCommandLineArgs(),
				s => s.Equals("--nowindow")))
			{
				await Task.Delay(500);
				this.MainWindow.Hide();
			}
		}

		[SuppressMessage("ReSharper", "FunctionNeverReturns")]
		private async Task HandleLanguageLoopAsync()
		{
			while (true)
			{
				this.LanguageManager.SetCurrentLanguage();
				await Task.Delay(100);
			}
		}

		#endregion
	}
}

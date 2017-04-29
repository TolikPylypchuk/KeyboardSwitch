using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using IWshRuntimeLibrary;

using KeyboardSwitch.Infrastructure;
using KeyboardSwitch.Properties;
using KeyboardSwitch.Services;
using KeyboardSwitch.UI;

namespace KeyboardSwitch
{
	[SuppressMessage("ReSharper", "UnusedVariable")]
	[SuppressMessage("ReSharper", "FunctionNeverReturns")]
	public class App : Application
	{
		[STAThread]
		static void Main(string[] args)
		{
			var wrapper = new SingleInstanceWrapper();
			wrapper.Run(args);
		}

		public HotKey HotKeyForward { get; set; }
		public HotKey HotKeyBackward { get; set; }

		public CultureInfo CurrentLanguage { get; private set; }

		public Dictionary<CultureInfo, StringBuilder> Languages
		{
			get;
			private set;
		}

		private object SyncRoot { get; } = new Object();

		public static Key GetKey(char value)
		{
			value = char.ToUpper(value);
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(value) != -1)
			{
				return (Key)(value - 'A' + (int)Key.A);
			}

			throw new ArgumentOutOfRangeException(
				nameof(value),
				"Only English letters can be set here.");
		}

		public static KeyModifier GetKeyModifiers()
		{
			switch (Settings.Default.KeyModifiers)
			{
				case "CS":
					return KeyModifier.Ctrl | KeyModifier.Shift;
				case "AS":
					return KeyModifier.Alt | KeyModifier.Shift;
			}

			return KeyModifier.Ctrl | KeyModifier.Alt;
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

		public void SetStartMenuShortcut(bool add)
		{
			string shortcutLocation = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
				"Programs",
				"Keyboard Layout Switch.lnk");

			SetShortcut(shortcutLocation, add, false);
		}

		public void SetStartupShortcut(bool add)
		{
			string shortcutLocation = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Startup),
				"Keyboard Layout Switch.lnk");

            SetShortcut(shortcutLocation, add, true);
		}

		public void HotKeyPressed(HotKey key)
		{
			lock (SyncRoot)
			{
				CultureInfo toLang = null;

				var list = InputLanguageManager.Current.AvailableInputLanguages
					?.Cast<CultureInfo>().ToList();

				if (list == null)
				{
					return;
				}
				
				if (key == HotKeyBackward)
				{
					list.Reverse();
				}

				bool finished = false;
				foreach (var lang in list)
				{
					if (!finished)
					{
						finished = lang.Equals(CurrentLanguage);
					} else
					{
						toLang = lang;
						finished = false;
						break;
					}
				}

				if (finished)
				{
					toLang = list[0];
				}
				
				if (toLang == null)
				{
					return;
				}

				if (Clipboard.ContainsText())
				{
					string oldString =
						this.Languages[CurrentLanguage].ToString();
					string newString =
						this.Languages[toLang].ToString();

					string text = Clipboard.GetText();
					var result = new StringBuilder();

					foreach (char ch in text)
					{
						if (Char.IsWhiteSpace(ch) || Char.IsControl(ch))
						{
							result.Append(ch);
						} else
						{
							try
							{
								char newCh = newString[oldString.IndexOf(ch)];
								result.Append(newCh);
							} catch (OutOfMemoryException)
							{
								new ErrorWindow(
									null, "Out of memory.\n").ShowDialog();
								result = null;
								GC.Collect();
								return;
							} catch
							{
								result.Append(ch);
							}
						}
					}

					Clipboard.SetText(result.ToString());

					Debug.Write(
						$"In HotKeyPressed(): {this.CurrentLanguage} -> ");

					this.CurrentLanguage = toLang;
					this.ChangeLanguage();

					Debug.WriteLine(this.CurrentLanguage);
				}
			}
		}

		protected override async void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			var task = Task.Factory.StartNew(() =>
			{
				while (true)
				{
					this.HandleCurrentLanguage();
					Thread.Sleep(100);
				}
			});

			bool success = FileManager.TryRead(out var langs);
			this.Languages = langs;

			if (!success)
			{
				new ErrorWindow(
					null,
					"Cannot read character mappings.\n" +
					"You have to define them yourself.")
					.ShowDialog();
			}
			
			var modifiers = GetKeyModifiers();

			this.HotKeyForward = new HotKey(
				GetKey(Settings.Default.HotKeyForward),
				modifiers,
				this.HotKeyPressed);

			this.HotKeyBackward = new HotKey(
				GetKey(Settings.Default.HotKeyBackward),
				modifiers,
				this.HotKeyPressed);

			this.SetStartupShortcut(Settings.Default.RunOnStartup);
            this.SetStartMenuShortcut(Settings.Default.StartMenuIcon);

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

		private static void SetShortcut(
			string shortcutLocation,
			bool add,
			bool nowindow)
		{
			bool fileExists = System.IO.File.Exists(shortcutLocation);

			if (add && !fileExists)
			{
				string pathToExe = Assembly.GetEntryAssembly().Location;

				var shell = new WshShell();
				var shortcut = (IWshShortcut)shell.CreateShortcut(
					shortcutLocation);

				shortcut.Description = "Keyboard Layout Switch";
				shortcut.TargetPath = pathToExe;

				if (nowindow)
				{
					shortcut.Arguments = "--nowindow";
				}

				shortcut.Save();
			} else if (!add && fileExists)
			{
				System.IO.File.Delete(shortcutLocation);
			}
		}

		private void HandleCurrentLanguage()
		{
			lock (this.SyncRoot)
			{
				var layout = NativeMethods.GetKeyboardLayout(
					NativeMethods.GetWindowThreadProcessId(
						NativeMethods.GetForegroundWindow(), IntPtr.Zero));

				var currentCulture = new CultureInfo((short)layout.ToInt64());

				if (this.CurrentLanguage == null ||
					this.CurrentLanguage.LCID != currentCulture.LCID)
				{
					Debug.Write(
						"In HandleCurrentLanguage(): " +
						(CurrentLanguage?.ToString() ?? "null") + " -> ");

					this.CurrentLanguage = currentCulture;

					Debug.WriteLine(this.CurrentLanguage.ToString());
				}
			}
		}

		private void ChangeLanguage()
		{
			var tempLang = InputLanguageManager.Current.CurrentInputLanguage;
			InputLanguageManager.Current.CurrentInputLanguage = this.CurrentLanguage;

			var input = new StringBuilder(9);
			var layout = NativeMethods.GetKeyboardLayout(
				(uint)Process.GetCurrentProcess().Id);

			NativeMethods.GetKeyboardLayoutName(input);
			NativeMethods.PostMessage(
				NativeMethods.GetForegroundWindow(),
				0x0050,
				IntPtr.Zero,
				NativeMethods.LoadKeyboardLayout(input.ToString(), 1));

			InputLanguageManager.Current.CurrentInputLanguage = tempLang;
		}
	}
}

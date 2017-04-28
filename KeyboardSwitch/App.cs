using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic.ApplicationServices;
using IWshRuntimeLibrary;

namespace KeyboardSwitch
{
	class App : Application
	{
		[STAThread]
		static void Main(string[] args)
		{
			SingleInstanceWrapper wrapper = new SingleInstanceWrapper();
			wrapper.Run(args);
		}

		public HotKey HotKeyForward { get; set; }
		public HotKey HotKeyBackward { get; set; }

		public CultureInfo CurrentLanguage { get; private set; }
		public Dictionary<CultureInfo, StringBuilder> Languages { get; private set; }

		private object lockObj = new Object();

		public static Key GetKey(char value)
		{
			value = char.ToUpper(value);
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(value) != -1)
			{
				return (Key)(value - 'A' + (int)Key.A);
			}

			throw new ArgumentOutOfRangeException(
				"Only English letters can be set here.");
		}

		public static KeyModifier GetKeyModifiers()
		{
			switch (KeyboardSwitch.Properties.Settings.Default.KeyModifiers)
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
			SettingsWindow window = MainWindow as SettingsWindow;
			if (window != null)
			{
				window.BringToForeground();
			} else
			{
				BringWindowToForeground(MainWindow);
			}
		}

		public void SetStartMenuShortcut(bool add)
		{
			string shortcutLocation = Path.Combine(Environment.GetFolderPath(
				Environment.SpecialFolder.StartMenu), "Programs",
				"Keyboard Layout Switch.lnk");

			SetShortcut(shortcutLocation, add, false);
		}

		public void SetStartupShortcut(bool add)
		{
			string shortcutLocation = Path.Combine(Environment.GetFolderPath(
				Environment.SpecialFolder.Startup), "Keyboard Layout Switch.lnk");

            SetShortcut(shortcutLocation, add, true);
		}

		public void HotKeyPressed(HotKey key)
		{
			lock (lockObj)
			{
				CultureInfo toLang = null;

				ArrayList list = InputLanguageManager.Current.AvailableInputLanguages
					as ArrayList;

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
						toLang = lang as CultureInfo;
						finished = false;
						break;
					}
				}

				if (finished)
				{
					toLang = list[0] as CultureInfo;
				}

				if (Clipboard.ContainsText())
				{
					string oldString = Languages[CurrentLanguage].ToString();
					string newString = Languages[toLang].ToString();

					string text = Clipboard.GetText();
					StringBuilder result = new StringBuilder();
					foreach (char ch in text)
					{
						if (ch == ' ')
						{
							result.Append(' ');
						} else
						{
							try
							{
								char newCh = newString[oldString.IndexOf(ch)];
								result.Append(newCh == ' ' ? ch : newCh);
							} catch (OutOfMemoryException)
							{
								new ErrorWindow(null, "Out of memory.\n").ShowDialog();
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

					Debug.Write("In HotKeyPressed(): " +
						CurrentLanguage.ToString() + " -> ");
					CurrentLanguage = toLang;
					ChangeLanguage();
					Debug.WriteLine(CurrentLanguage.ToString());
				}
			}
		}

		protected override async void OnStartup(System.Windows.StartupEventArgs e)
		{
			base.OnStartup(e);

			ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;

			Task task = Task.Factory.StartNew(() =>
			{
				while (true)
				{
					HandleCurrentLanguage();
					Thread.Sleep(100);
				}
			});

			bool success = false;
			Languages = FileManager.Read(out success);
			if (!success)
			{
				new ErrorWindow(null, "Cannot read character mappings.\n" +
					"You have to define them yourself.").ShowDialog();
			}
			
			KeyModifier modifiers = GetKeyModifiers();

			HotKeyForward = new HotKey(
				GetKey(KeyboardSwitch.Properties.Settings.Default.HotKeyForward),
				modifiers, HotKeyPressed);

			HotKeyBackward = new HotKey(
				GetKey(KeyboardSwitch.Properties.Settings.Default.HotKeyBackward),
				modifiers, HotKeyPressed);

			SetStartupShortcut(KeyboardSwitch.Properties.Settings.Default.RunOnStartup);
            SetStartMenuShortcut(KeyboardSwitch.Properties.Settings.Default.StartMenuIcon);

			MainWindow = new SettingsWindow();
			MainWindow.Show();

			if (Array.Exists(Environment.GetCommandLineArgs(),
				s => s.Equals("-nowindow")))
			{
				await Task.Delay(500);
				MainWindow.Hide();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			HotKeyForward.Dispose();
			HotKeyBackward.Dispose();
			base.OnExit(e);
		}

		private void SetShortcut(string shortcutLocation, bool add, bool nowindow)
		{
			bool fileExists = System.IO.File.Exists(shortcutLocation);

			if (add && !fileExists)
			{
				string pathToExe = Assembly.GetEntryAssembly().Location;

				WshShell shell = new WshShell();
				IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(
					shortcutLocation);

				shortcut.Description = "Keyboard Layout Switch";
				shortcut.TargetPath = pathToExe;
				if (nowindow)
				{
					shortcut.Arguments = "-nowindow";
				}
				shortcut.Save();
			} else if (!add && fileExists)
			{
				System.IO.File.Delete(shortcutLocation);
			}
		}

		private void HandleCurrentLanguage()
		{
			lock (lockObj)
			{
				IntPtr l = NativeMethods.GetKeyboardLayout(
					NativeMethods.GetWindowThreadProcessId(
						NativeMethods.GetForegroundWindow(), IntPtr.Zero));
				CultureInfo currentCulture = new CultureInfo((short)l.ToInt64());
				if (CurrentLanguage == null ||
					CurrentLanguage.LCID != currentCulture.LCID)
				{
					Debug.Write("In HandleCurrentLanguage(): " +
						(CurrentLanguage == null ? "null" :
						CurrentLanguage.ToString()) + " -> ");
					CurrentLanguage = currentCulture;
					Debug.WriteLine(CurrentLanguage.ToString());
				}
			}
		}

		private void ChangeLanguage()
		{
			CultureInfo tempLang = InputLanguageManager.Current.CurrentInputLanguage;
			InputLanguageManager.Current.CurrentInputLanguage = CurrentLanguage;
			StringBuilder input = new StringBuilder(9);
			IntPtr layout = NativeMethods.GetKeyboardLayout(
				(uint)Process.GetCurrentProcess().Id);
			NativeMethods.GetKeyboardLayoutName(input);
			NativeMethods.PostMessage(NativeMethods.GetForegroundWindow(), 0x0050,
				IntPtr.Zero, NativeMethods.LoadKeyboardLayout(input.ToString(), 1));
			InputLanguageManager.Current.CurrentInputLanguage = tempLang;
		}
	}

	class SingleInstanceWrapper : WindowsFormsApplicationBase
	{
		private App app;

		public SingleInstanceWrapper()
		{
			IsSingleInstance = true;
		}

		protected override bool OnStartup(
			Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
		{
			app = new App();
			app.Run();

			return false;
		}

		protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
		{
			app.ProcessNextInstance();
		}
	}

	class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern IntPtr GetKeyboardLayout(uint idThread);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int GetKeyboardLayoutName(StringBuilder pwszKLID);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint flags);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool PostMessage(IntPtr hhwnd, uint msg, IntPtr wparam, IntPtr lparam);

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

		[DllImport("user32.dll")]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

		[DllImport("user32.dll")]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
	}
}

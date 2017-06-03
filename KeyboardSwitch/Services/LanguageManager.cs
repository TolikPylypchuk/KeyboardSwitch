using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using KeyboardSwitch.UI;

using static KeyboardSwitch.Infrastructure.NativeFunctions;

namespace KeyboardSwitch.Services
{
	public class LanguageManager
	{
		private LanguageManager() { }

		public static LanguageManager Current { get; } = new LanguageManager();

		public CultureInfo CurrentLanguage { get; private set; }
		public Dictionary<CultureInfo, StringBuilder> Languages { get; set; }

		private object SyncRoot { get; } = new Object();

		public void SetCurrentLanguage()
		{
			lock (this.SyncRoot)
			{
				var layout = GetKeyboardLayout(
					GetWindowThreadProcessId(
						GetForegroundWindow(), IntPtr.Zero));

				var currentCulture = new CultureInfo((short)layout.ToInt64());

				if (this.CurrentLanguage == null ||
				    this.CurrentLanguage.LCID != currentCulture.LCID)
				{
					Debug.Write(
						"In HandleCurrentLanguage(): " +
						(this.CurrentLanguage?.ToString() ?? "null") + " -> ");

					this.CurrentLanguage = currentCulture;

					Debug.WriteLine(this.CurrentLanguage.ToString());
				}
			}
		}

		public void ChangeLanguage()
		{
			var lang = InputLanguageManager.Current.CurrentInputLanguage;
			InputLanguageManager.Current.CurrentInputLanguage = this.CurrentLanguage;

			var input = new StringBuilder(9);
			GetKeyboardLayout((uint)Process.GetCurrentProcess().Id);

			GetKeyboardLayoutName(input);

			PostMessage(
				GetForegroundWindow(),
				0x0050,
				IntPtr.Zero,
				LoadKeyboardLayout(input.ToString(), 1));

			InputLanguageManager.Current.CurrentInputLanguage = lang;
		}

		public void SwitchText(bool forward)
		{
			lock (this.SyncRoot)
			{
				CultureInfo toLang = null;

				var list = InputLanguageManager.Current.AvailableInputLanguages
					?.Cast<CultureInfo>().ToList();

				if (list == null)
				{
					return;
				}

				if (!forward)
				{
					list.Reverse();
				}

				bool finished = false;
				foreach (var lang in list)
				{
					if (!finished)
					{
						finished = lang.Equals(
							this.CurrentLanguage);
					}
					else
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
						this.Languages[this.CurrentLanguage].ToString();
					string newString = this.Languages[toLang].ToString();

					string text = Clipboard.GetText();
					var result = new StringBuilder();

					foreach (char ch in text)
					{
						if (Char.IsWhiteSpace(ch) || Char.IsControl(ch))
						{
							result.Append(ch);
						}
						else
						{
							try
							{
								char newCh = newString[oldString.IndexOf(ch)];
								result.Append(newCh);
							}
							catch (OutOfMemoryException)
							{
								new ErrorWindow(
									null, "Out of memory.\n").ShowDialog();
								result = null;
								GC.Collect();
								return;
							}
							catch
							{
								result.Append(ch);
							}
						}
					}

					Clipboard.SetText(result.ToString());

					Debug.Write(
						$"In SwitchText(): {this.CurrentLanguage} -> ");

					this.CurrentLanguage = toLang;
					this.ChangeLanguage();

					Debug.WriteLine(this.CurrentLanguage);
				}
			}
		}
	}
}

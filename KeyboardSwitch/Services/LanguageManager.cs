using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace KeyboardSwitch.Services
{
	public class LanguageManager
	{
		public LanguageManager(
			IInputLanguageManager inputLanguageManager,
			ILayoutManager layoutManager)
		{
			this.InputLanguageManager = inputLanguageManager;
			this.LayoutManager = layoutManager;
		}
		
		public CultureInfo CurrentLanguage { get; private set; }
		public Dictionary<CultureInfo, StringBuilder> Languages { get; set; }

		public IInputLanguageManager InputLanguageManager { get; }
		public ILayoutManager LayoutManager { get; }

		private object SyncRoot { get; } = new Object();
		
		public void SetCurrentLanguage()
		{
			lock (this.SyncRoot)
			{
				var currentLanguage = this.LayoutManager.GetCurrentLayout();

				if (currentLanguage != null &&
					(this.CurrentLanguage == null ||
				    this.CurrentLanguage.LCID != currentLanguage.LCID))
				{
					Debug.Write(
						$"In {nameof(this.SetCurrentLanguage)}(): " +
						(this.CurrentLanguage?.ToString() ?? "null") + " -> ");

					this.CurrentLanguage = currentLanguage;

					Debug.WriteLine(this.CurrentLanguage.ToString());
				}
			}
		}

		public void ChangeLanguage()
			=> this.LayoutManager.SetCurrentLayout(this.CurrentLanguage);

		public void SwitchText(ITextManager textManager, bool forward)
		{
			lock (this.SyncRoot)
			{
				CultureInfo targetLang = null;

				var langs = this.InputLanguageManager.InputLanguages.ToList();
				
				if (!forward)
				{
					langs.Reverse();
				}

				bool finished = false;
				foreach (var lang in langs)
				{
					if (!finished)
					{
						finished = lang.Equals(
							this.CurrentLanguage);
					} else
					{
						targetLang = lang;
						finished = false;
						break;
					}
				}

				if (finished)
				{
					targetLang = langs[0];
				}

				if (targetLang == null || !textManager.HasText)
				{
					return;
				}
				
				string oldString =
					this.Languages[this.CurrentLanguage].ToString();
				string newString = this.Languages[targetLang].ToString();

				string text = textManager.GetText();

				if (!String.IsNullOrEmpty(text))
				{
					var result = new StringBuilder();

					foreach (char ch in text)
					{
						try
						{
							if (Char.IsWhiteSpace(ch) || Char.IsControl(ch) ||
								!oldString.Contains(ch))
							{
								result.Append(ch);
							} else
							{
								char newCh = newString[oldString.IndexOf(ch)];
								result.Append(newCh);
							}
						} catch (OutOfMemoryException)
						{
							MessageBox.Show(
								"Out of memory.",
								"Keyboard Layout Switch - Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
							return;
						} catch
						{
							MessageBox.Show(
								"An unknown error occured.",
								"Keyboard Layout Switch - Error",
								MessageBoxButton.OK,
								MessageBoxImage.Error);
							return;
						}
					}

					textManager.SetText(result.ToString());

					Debug.WriteLine(
						$"In {nameof(this.SwitchText)}(): " +
						$"{text} -> {result}");

					this.CurrentLanguage = targetLang;
					this.ChangeLanguage();
				}
			}
		}
	}
}

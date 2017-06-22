﻿using System;
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
		#region Constructors

		private LanguageManager() { }

		#endregion
		
		#region Properties

		public static LanguageManager Current { get; } = new LanguageManager();

		public CultureInfo CurrentLanguage { get; private set; }
		public Dictionary<CultureInfo, StringBuilder> Languages { get; set; }

		public IInputLanguageManager InputLanguageManager { get; set; }
		public ILayoutManager LayoutManager { get; set; }

		private object SyncRoot { get; } = new Object();

		#endregion

		#region Methods

		public void SetCurrentLanguage()
		{
			lock (this.SyncRoot)
			{
				var currentLanguage = this.LayoutManager.GetCurrentLayout();

				if (this.CurrentLanguage == null ||
				    this.CurrentLanguage.LCID != currentLanguage.LCID)
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
				var result = new StringBuilder();

				foreach (char ch in text)
				{
					if (Char.IsWhiteSpace(ch) || Char.IsControl(ch) ||
						!oldString.Contains(ch))
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
				}

				textManager.SetText(result.ToString());

				Debug.WriteLine(
					$"In {nameof(this.SwitchText)}(): " +
					$"{text} -> {result}");

				this.CurrentLanguage = targetLang;
				this.ChangeLanguage();
			}
		}

		#endregion
	}
}

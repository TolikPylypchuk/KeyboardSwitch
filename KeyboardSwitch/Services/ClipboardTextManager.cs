using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace KeyboardSwitch.Services
{
	public class ClipboardTextManager : ITextManager
	{
		public bool HasText
		{
			get
			{
				try
				{
					return Clipboard.ContainsText();
				} catch (COMException)
				{
					return false;
				}
			}
		}

		public string GetText()
		{
			try
			{
				return Clipboard.GetText();
			} catch (COMException)
			{
				return String.Empty;
			}
		}

		public void SetText(string text)
		{
			try
			{
				Clipboard.SetText(text);
			} catch (COMException) { }
		}
	}
}

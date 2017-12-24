﻿using System.Windows;

namespace KeyboardSwitch.Services
{
	public class ClipboardTextManager : ITextManager
	{
		private ClipboardTextManager() { }

		public static ClipboardTextManager Current { get; } =
			new ClipboardTextManager();

		public bool HasText => Clipboard.ContainsText();

		public string GetText() => Clipboard.GetText();
		public void SetText(string text) => Clipboard.SetText(text);
	}
}

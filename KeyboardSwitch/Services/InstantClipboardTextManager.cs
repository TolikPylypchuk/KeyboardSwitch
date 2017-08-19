using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Threading;

using WindowsInput;
using WindowsInput.Native;

namespace KeyboardSwitch.Services
{
	[ExcludeFromCodeCoverage]
	public class InstantClipboardTextManager : ITextManager
	{
		private InstantClipboardTextManager() { }

		public static InstantClipboardTextManager Current { get; } =
			new InstantClipboardTextManager();

		public IKeyboardSimulator Keyboard { get; set; }

		public bool HasText => true;

		public string GetText()
		{
			this.Keyboard.ModifiedKeyStroke(
				VirtualKeyCode.CONTROL, VirtualKeyCode.VK_X);

			Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
				new Action(() => { }));

			string result = Clipboard.ContainsText()
				? Clipboard.GetText()
				: String.Empty;

			Debug.WriteLine($"In {nameof(this.GetText)}. Copied {result}.");

			return result;
		}

		public void SetText(string text)
		{
			Clipboard.SetText(text);
			
			this.Keyboard.ModifiedKeyStroke(
				VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

			Debug.WriteLine($"In {nameof(this.SetText)}. Pasted {text}.");
		}
	}
}

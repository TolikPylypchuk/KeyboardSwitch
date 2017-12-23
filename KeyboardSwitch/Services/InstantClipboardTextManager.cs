using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using WindowsInput;
using WindowsInput.Native;

using KeyboardSwitch.Interop;
using KeyboardSwitch.Properties;

namespace KeyboardSwitch.Services
{
	[ExcludeFromCodeCoverage]
	public class InstantClipboardTextManager : ITextManager
	{
		private const int SLEEP_TIME = 50;

		private InstantClipboardTextManager() { }

		public static InstantClipboardTextManager Current { get; } =
			new InstantClipboardTextManager();

		public IKeyboardSimulator Keyboard { get; set; }

		public bool HasText => true;

		public string GetText()
		{
			this.ModifiersUp();

			Thread.Sleep(SLEEP_TIME);
			
			this.Keyboard.ModifiedKeyStroke(
				VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

			Application.Current.Dispatcher.Invoke(
				DispatcherPriority.Background,
				new Action(() => { }));

			string result = Clipboard.ContainsText()
				? Clipboard.GetText()
				: String.Empty;

			Debug.WriteLine($"In {nameof(this.GetText)}. Copied {result}.");

			return result;
		}

		public void SetText(string text)
		{
			this.ModifiersUp();

			Application.Current.Dispatcher.Invoke(
				DispatcherPriority.Background,
				new Action(() => { }));

			Clipboard.SetText(text);

			Thread.Sleep(SLEEP_TIME);

			this.Keyboard.ModifiedKeyStroke(
				VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

			Debug.WriteLine($"In {nameof(this.SetText)}. Pasted {text}.");
		}

		private void ModifiersUp()
		{
			if ((Settings.Default.KeyModifiers & KeyModifier.Alt) != KeyModifier.None)
			{
				this.Keyboard.KeyUp(VirtualKeyCode.MENU);
			}

			if ((Settings.Default.KeyModifiers & KeyModifier.Ctrl) != KeyModifier.None)
			{
				this.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
			}

			if ((Settings.Default.KeyModifiers & KeyModifier.Shift) != KeyModifier.None)
			{
				this.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
			}
		}
	}
}

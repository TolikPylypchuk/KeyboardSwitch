﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

using WindowsInput;
using WindowsInput.Native;

using KeyboardSwitch.Interop;
using KeyboardSwitch.Properties;

namespace KeyboardSwitch.Services
{
	public class InstantClipboardTextManager : ITextManager
	{
		private const int SLEEP_TIME = 50;

		public InstantClipboardTextManager(IKeyboardSimulator keyboard)
		{
			this.Keyboard = keyboard;
		}
		
		public IKeyboardSimulator Keyboard { get; }

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

			string result = String.Empty;

			try
			{
				if (Clipboard.ContainsText())
				{
					result = Clipboard.GetText();
				}

				Debug.WriteLine($"In {nameof(this.GetText)}. Copied {result}.");
			} catch (COMException) { }
			
			return result;
		}

		public void SetText(string text)
		{
			this.ModifiersUp();

			Application.Current.Dispatcher.Invoke(
				DispatcherPriority.Background,
				new Action(() => { }));

			try
			{
				Clipboard.SetText(text);

				this.Keyboard.ModifiedKeyStroke(
					VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

				Debug.WriteLine($"In {nameof(this.SetText)}. Pasted {text}.");

				Thread.Sleep(SLEEP_TIME);
			} catch (COMException) { }
		}

		private void ModifiersUp()
		{
			if ((Settings.Default.InstantKeyModifiers & KeyModifier.Alt) != KeyModifier.None)
			{
				this.Keyboard.KeyUp(VirtualKeyCode.MENU);
			}

			if ((Settings.Default.InstantKeyModifiers & KeyModifier.Ctrl) != KeyModifier.None)
			{
				this.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
			}

			if ((Settings.Default.InstantKeyModifiers & KeyModifier.Shift) != KeyModifier.None)
			{
				this.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
			}
		}
	}
}

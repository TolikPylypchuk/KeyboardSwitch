using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;

namespace KeyboardSwitch
{
	public class HotKey : IDisposable
	{
		private static Dictionary<int, HotKey> dictHotKeyToCallBackProc;

		private bool disposed = false;

		public Key Key { get; private set; }
		public KeyModifier KeyModifiers { get; private set; }
		public Action<HotKey> Action { get; private set; }
		public int Id { get; set; }

		public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action,
			bool register = true)
		{
			Key = k;
			KeyModifiers = keyModifiers;
			Action = action;
			if (register)
			{
				Register();
			}
		}

		public bool Register()
		{
			int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
			Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
			bool result = NativeMethods.RegisterHotKey(IntPtr.Zero, Id,
				(uint)KeyModifiers, (uint)virtualKeyCode);

			if (dictHotKeyToCallBackProc == null)
			{
				dictHotKeyToCallBackProc = new Dictionary<int, HotKey>();
				ComponentDispatcher.ThreadFilterMessage +=
					new ThreadMessageEventHandler(
						ComponentDispatcherThreadFilterMessage);
			}

			dictHotKeyToCallBackProc.Add(Id, this);

			return result;
		}

		public void Unregister()
		{
			HotKey hotKey;
			if (dictHotKeyToCallBackProc.TryGetValue(Id, out hotKey))
			{
				NativeMethods.UnregisterHotKey(IntPtr.Zero, Id);
				dictHotKeyToCallBackProc.Remove(Id);
            }
		}

		private static void ComponentDispatcherThreadFilterMessage(
			ref MSG msg, ref bool handled)
		{
			if (!handled)
			{
				if (msg.message == 0x0312)
				{
					HotKey hotKey;

					if (dictHotKeyToCallBackProc.TryGetValue(
						(int)msg.wParam, out hotKey))
					{
						if (hotKey.Action != null)
						{
							hotKey.Action.Invoke(hotKey);
						}
						handled = true;
					}
				}
			}
		}
		
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					Unregister();
				}

				disposed = true;
			}
		}
	}

	[Flags]
	public enum KeyModifier
	{
		None = 0x0000,
		Alt = 0x0001,
		Ctrl = 0x0002,
		NoRepeat = 0x4000,
		Shift = 0x0004,
		Win = 0x0008
	}
}
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Interop;

using KeyboardSwitch.Infrastructure;

namespace KeyboardSwitch.Services
{
	public class HotKey : IDisposable
	{
		private static Dictionary<int, HotKey> dictHotKeyToCallBackProc;

		private bool disposed;

		public Key Key { get; private set; }
		public KeyModifier KeyModifiers { get; private set; }
		public Action<HotKey> Action { get; private set; }
		public int Id { get; set; }

		public HotKey(
			Key k,
			KeyModifier keyModifiers,
			Action<HotKey> action,
			bool register = true)
		{
			this.Key = k;
			this.KeyModifiers = keyModifiers;
			this.Action = action;

			if (register)
			{
				this.Register();
			}
		}

		public bool Register()
		{
			int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
			this.Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);

			bool result = NativeMethods.RegisterHotKey(
				IntPtr.Zero, Id, (uint)KeyModifiers, (uint)virtualKeyCode);

			if (dictHotKeyToCallBackProc == null)
			{
				dictHotKeyToCallBackProc = new Dictionary<int, HotKey>();
				ComponentDispatcher.ThreadFilterMessage +=
						ComponentDispatcherThreadFilterMessage;
			}

			dictHotKeyToCallBackProc.Add(Id, this);

			return result;
		}

		public void Unregister()
		{
			if (dictHotKeyToCallBackProc.TryGetValue(Id, out HotKey _))
			{
				NativeMethods.UnregisterHotKey(IntPtr.Zero, Id);
				dictHotKeyToCallBackProc.Remove(Id);
			}
		}

		private static void ComponentDispatcherThreadFilterMessage(
			ref MSG msg,
			ref bool handled)
		{
			if (!handled)
			{
				if (msg.message == 0x0312)
				{
					if (dictHotKeyToCallBackProc.TryGetValue(
						(int)msg.wParam, out HotKey hotKey))
					{
						hotKey.Action?.Invoke(hotKey);
						handled = true;
					}
				}
			}
		}
		
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.Unregister();
				}

				this.disposed = true;
			}
		}
	}
}
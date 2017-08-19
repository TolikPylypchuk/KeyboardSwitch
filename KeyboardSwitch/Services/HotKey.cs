using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Windows.Interop;

using KeyboardSwitch.Interop;

namespace KeyboardSwitch.Services
{
	[ExcludeFromCodeCoverage]
	public class HotKey : IDisposable
	{
		#region Fields

		private static Dictionary<int, HotKey> dictHotKeyToCallBackProc;
		private bool disposed;

		#endregion

		#region Constructor and destructor

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

		~HotKey() => this.Dispose(false);

		#endregion

		#region Properties

		public Key Key { get; private set; }
		public KeyModifier KeyModifiers { get; private set; }
		public Action<HotKey> Action { get; private set; }
		public int Id { get; set; }

		#endregion

		#region Methods

		public bool Register()
		{
			int virtualKeyCode = KeyInterop.VirtualKeyFromKey(this.Key);
			this.Id = virtualKeyCode + (int)this.KeyModifiers * 0x10000;

			bool result = NativeFunctions.RegisterHotKey(
				IntPtr.Zero,
				this.Id,
				(uint)this.KeyModifiers,
				(uint)virtualKeyCode);

			if (dictHotKeyToCallBackProc == null)
			{
				dictHotKeyToCallBackProc = new Dictionary<int, HotKey>();
				ComponentDispatcher.ThreadFilterMessage +=
					ComponentDispatcherThreadFilterMessage;
			}

			dictHotKeyToCallBackProc.Add(this.Id, this);

			return result;
		}

		public void Unregister()
		{
			if (dictHotKeyToCallBackProc.ContainsKey(this.Id))
			{
				NativeFunctions.UnregisterHotKey(IntPtr.Zero, this.Id);
				dictHotKeyToCallBackProc.Remove(this.Id);
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

		#endregion
	}
}

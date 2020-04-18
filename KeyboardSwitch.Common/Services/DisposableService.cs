using System;
using System.Runtime.CompilerServices;

namespace KeyboardSwitch.Common.Services
{
    public abstract class DisposableService
    {
        protected bool Disposed = false;

        protected void ThrowIfDisposed([CallerMemberName] string? method = null)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException($"Cannot call {method} - the service is disposed");
            }
        }
    }
}

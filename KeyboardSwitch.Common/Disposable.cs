using System;
using System.Runtime.CompilerServices;

namespace KeyboardSwitch.Common
{
    public abstract class Disposable : IDisposable
    {
        ~Disposable() =>
            this.Dispose(false);

        protected bool Disposed { get; set; } = false;

        public void Dispose()
        {
            if (!this.Disposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);

                this.Disposed = true;
            }
        }

        protected abstract void Dispose(bool disposing);

        protected void ThrowIfDisposed([CallerMemberName] string? method = null)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(
                    this.GetType().Name, $"Cannot call {method} - the service is disposed");
            }
        }
    }
}

using System.Diagnostics.CodeAnalysis;

namespace KeyboardSwitch.Core;

public abstract class DisposableService : IDisposable
{
    [ExcludeFromCodeCoverage]
    ~DisposableService() =>
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

    protected void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(this.Disposed, this);
}

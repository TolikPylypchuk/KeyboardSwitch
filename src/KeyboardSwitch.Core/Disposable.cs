namespace KeyboardSwitch.Core;

using System.Diagnostics.CodeAnalysis;

public abstract class Disposable : IDisposable
{
    [ExcludeFromCodeCoverage]
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

    protected void ThrowIfDisposed() =>
        ObjectDisposedException.ThrowIf(this.Disposed, this);
}

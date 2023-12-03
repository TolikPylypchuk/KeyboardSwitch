namespace KeyboardSwitch.Core;

public abstract class AsyncDisposable : Disposable, IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        if (!this.Disposed)
        {
            await this.DisposeAsyncCore();
            this.Dispose(false);
            GC.SuppressFinalize(this);

            this.Disposed = true;
        }
    }

    protected abstract ValueTask DisposeAsyncCore();

    protected override void Dispose(bool disposing)
    { }
}

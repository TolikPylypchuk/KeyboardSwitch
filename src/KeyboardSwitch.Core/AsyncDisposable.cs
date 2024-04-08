namespace KeyboardSwitch.Core;

public sealed class AsyncDisposable(Func<ValueTask> dispose) : IAsyncDisposable
{
    public ValueTask DisposeAsync() =>
        dispose();
}

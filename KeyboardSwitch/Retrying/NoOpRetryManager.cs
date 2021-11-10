namespace KeyboardSwitch.Retrying;

public sealed class NoOpRetryManager : IRetryManager
{
    public Task DoWithRetrying(Func<Task> action) =>
        action();
}

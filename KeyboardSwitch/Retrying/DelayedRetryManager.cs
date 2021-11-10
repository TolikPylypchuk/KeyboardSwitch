namespace KeyboardSwitch.Retrying;

public sealed class DelayedRetryManager : IRetryManager
{
    private readonly IImmutableList<TimeSpan> delays;
    private readonly ILogger<DelayedRetryManager> logger;

    public DelayedRetryManager(IImmutableList<TimeSpan> delays, ILogger<DelayedRetryManager> logger)
    {
        if (delays == null)
        {
            throw new ArgumentNullException(nameof(delays));
        }

        if (delays.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(delays), "The list of delays cannot be empty");
        }

        this.delays = delays;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task DoWithRetrying(Func<Task> action) =>
        this.DoWithRetrying(action, 0);

    private async Task DoWithRetrying(Func<Task> action, int attempt)
    {
        try
        {
            await action();
        } catch (Exception e)
        {
            if (attempt < delays.Count - 1)
            {
                this.logger.LogWarning(e, $"Exception in a retryable action on attempt {attempt + 1}: {e.Message}");

                await Task.Delay(this.delays[attempt]);
                await this.DoWithRetrying(action, attempt + 1);
            } else
            {
                this.logger.LogError(
                    e, $"Exception in a retryable action on last attempt {attempt + 1}: {e.Message}");
                throw;
            }
        }
    }
}

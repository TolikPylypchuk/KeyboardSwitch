using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Retrying
{
    public sealed class DelayedRetryManager : IRetryManager
    {
        private readonly IImmutableList<TimeSpan> delays;
        private readonly List<Func<Exception, bool>> nonRetryingExceptions = new();
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

        public void DoNotRetryWhen(Func<Exception, bool> predicate) =>
            this.nonRetryingExceptions.Add(predicate ?? throw new ArgumentNullException(nameof(predicate)));

        private async Task DoWithRetrying(Func<Task> action, int attempt)
        {
            try
            {
                await action();
            } catch (Exception e)
            {
                if (nonRetryingExceptions.Any(predicate => predicate(e)))
                {
                    this.logger.LogError(e, $"Non-retryable exception: {e.Message}");
                    throw;
                }

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
}

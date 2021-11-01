using System;
using System.Threading.Tasks;

namespace KeyboardSwitch.Retrying
{
    public sealed class NoOpRetryManager : IRetryManager
    {
        public Task DoWithRetrying(Func<Task> action) =>
            action();

        public void DoNotRetryWhen(Func<Exception, bool> predicate)
        { }
    }
}

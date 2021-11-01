using System;
using System.Threading.Tasks;

namespace KeyboardSwitch.Retrying
{
    public interface IRetryManager
    {
        Task DoWithRetrying(Func<Task> action);
    }
}

using System.Threading.Tasks;

using KeyboardSwitch.Core.Services;

namespace KeyboardSwitch.Linux.Services
{
    public sealed class StartupService : IStartupService
    {
        public Task ConfigureStartupAsync(bool startup) =>
            Task.CompletedTask;

        public bool IsStartupConfigured() =>
            false;
    }
}

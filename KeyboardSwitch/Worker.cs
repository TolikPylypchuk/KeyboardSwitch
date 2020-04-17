using System.Threading;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyboardSwitch
{
    public class Worker : BackgroundService
    {
        private readonly IKeyboardHookService keyboardHook;
        private readonly ILogger<Worker> logger;

        public Worker(
            IKeyboardHookService keyboardHook,
            ILogger<Worker> logger)
        {
            this.keyboardHook = keyboardHook;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                this.logger.LogTrace("Configuring the keyboard switch service");

                this.RegisterHotKeys();

                this.logger.LogTrace("Starting the service execution");

                await this.keyboardHook.WaitForMessagesAsync(token);
            } finally
            {
                this.logger.LogTrace("Stopping the service");
                await BlobCache.Shutdown();
            }
        }

        private void RegisterHotKeys()
        {

        }
    }
}

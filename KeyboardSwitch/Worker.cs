using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyboardSwitch
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IKeyboardHookService keyboardHookService;

        public Worker(ILogger<Worker> logger, IKeyboardHookService keyboardHookService)
        {
            this.logger = logger;
            this.keyboardHookService = keyboardHookService;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            this.logger.LogTrace("Starting the service execution");

            await this.keyboardHookService.WaitForMessagesAsync(token);
        }
    }
}

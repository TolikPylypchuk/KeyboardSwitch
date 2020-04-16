using System.Threading;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyboardSwitch
{
    public class Worker : BackgroundService
    {
        private readonly IKeyboardHookService keyboardHook;
        private readonly ITextService text;
        private readonly ILogger<Worker> logger;

        public Worker(
            IKeyboardHookService keyboardHook,
            ITextService text,
            ILogger<Worker> logger)
        {
            this.keyboardHook = keyboardHook;
            this.text = text;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            this.logger.LogTrace("Starting the service execution");

            await this.keyboardHook.WaitForMessagesAsync(token);
        }
    }
}

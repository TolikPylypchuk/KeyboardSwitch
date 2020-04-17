using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using Akavache;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KeyboardSwitch
{
    public class Worker : BackgroundService
    {
        private readonly IKeyboardHookService keyboardHookService;
        private readonly ISwitchService switchService;
        private readonly ILogger<Worker> logger;

        public Worker(
            IKeyboardHookService keyboardHookService,
            ISwitchService switchService,
            ILogger<Worker> logger)
        {
            this.keyboardHookService = keyboardHookService;
            this.switchService = switchService;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                this.logger.LogTrace("Configuring the keyboard switch service");

                this.RegisterHotKeys();

                this.logger.LogTrace("Starting the service execution");

                await this.keyboardHookService.WaitForMessagesAsync(token);
            } finally
            {
                this.logger.LogTrace("Stopping the service");
                await BlobCache.Shutdown();
            }
        }

        private void RegisterHotKeys()
        {
            this.keyboardHookService.RegisterHotKey(ModifierKeys.Alt | ModifierKeys.Ctrl, 0x48);
            this.keyboardHookService.RegisterHotKey(ModifierKeys.Alt | ModifierKeys.Ctrl, 0x4A);

            this.keyboardHookService.HotKeyPressed
                .Select(hotKey => hotKey.VirtualKeyCode == 0x48 ? SwitchDirection.Forward : SwitchDirection.Backward)
                .SubscribeAsync(this.switchService.SwitchTextAsync);
        }
    }
}

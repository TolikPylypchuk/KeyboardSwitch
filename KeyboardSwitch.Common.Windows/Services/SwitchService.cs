using System;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Windows.Services
{
    internal class SwitchService : ISwitchService
    {
        private readonly ITextService textService;
        private readonly ILayoutService layoutService;
        private readonly ILogger<SwitchService> logger;

        public SwitchService(ITextService text, ILayoutService layoutService, ILogger<SwitchService> logger)
        {
            this.textService = text;
            this.layoutService = layoutService;
            this.logger = logger;
        }

        public async Task SwitchTextAsync(SwitchDirection direction)
        {
            this.logger.LogDebug($"Switching the text {direction.AsString()}");

            layoutService.SwitchForegroundProcessLayout(direction);

            await this.textService.SetTextAsync(await this.textService.GetTextAsync() ?? String.Empty);
        }
    }
}

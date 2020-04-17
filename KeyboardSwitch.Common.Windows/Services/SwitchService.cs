using System;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Windows.Services
{
    internal class SwitchService : ISwitchService
    {
        private readonly ITextService textService;
        private readonly ILogger<SwitchService> logger;

        public SwitchService(ITextService text, ILogger<SwitchService> logger)
        {
            this.textService = text;
            this.logger = logger;
        }

        public async Task SwitchTextAsync(SwitchDirection direction)
        {
            this.logger.LogTrace("Switching the text");
            await this.textService.SetTextAsync(await this.textService.GetTextAsync() ?? String.Empty);
        }
    }
}

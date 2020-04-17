using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Windows.Services
{
    internal class SwitchService : ISwitchService
    {
        private readonly ITextService text;
        private readonly ILogger<SwitchService> logger;

        public SwitchService(ITextService text, ILogger<SwitchService> logger)
        {
            this.text = text;
            this.logger = logger;
        }

        public Task SwitchTextAsync(SwitchDirection direction)
        {
            this.logger.LogTrace("Switching the text");
            return Task.CompletedTask;
        }
    }
}

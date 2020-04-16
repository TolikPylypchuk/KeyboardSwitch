using System.Threading.Tasks;
using System.Windows;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Windows.Services
{
    public class ClipboardTextService : ITextService
    {
        private readonly ILogger<ClipboardTextService> logger;

        public ClipboardTextService(ILogger<ClipboardTextService> logger)
            => this.logger = logger;

        public Task<string?> GetTextAsync()
        {
            this.logger.LogTrace("Getting the text from the clipboard.");
            return Task.FromResult(Clipboard.ContainsText() ? Clipboard.GetText() : null);
        }

        public Task SetTextAsync(string text)
        {
            this.logger.LogTrace("Setting the text into the clipboard.");
            return Task.CompletedTask;
        }
    }
}

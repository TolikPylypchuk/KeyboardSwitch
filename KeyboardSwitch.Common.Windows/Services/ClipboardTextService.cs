using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Windows.Services
{
    internal class ClipboardTextService : ITextService
    {
        private readonly IInputSimulator input;
        private readonly ISettingsService settingsService;
        private readonly ILogger<ClipboardTextService> logger;

        public ClipboardTextService(
            IInputSimulator input,
            ISettingsService settingsService,
            ILogger<ClipboardTextService> logger)
        {
            this.input = input;
            this.settingsService = settingsService;
            this.logger = logger;
        }

        public async Task<string?> GetTextAsync()
        {
            this.logger.LogDebug("Getting the text from the clipboard");

            var settings = await this.settingsService.GetAppSettingsAsync();

            return await TaskUtils.RunSTATask(() =>
                {
                    if (settings.InstantSwitching)
                    {
                        this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                        Thread.Sleep(50);
                    }

                    return Clipboard.ContainsText() ? Clipboard.GetText() : null;
                });
        }

        public async Task SetTextAsync(string text)
        {
            this.logger.LogDebug("Setting the text into the clipboard");

            var settings = await this.settingsService.GetAppSettingsAsync();

            await TaskUtils.RunSTATask(() =>
                {
                    Clipboard.SetText(text);

                    if (settings.InstantSwitching)
                    {
                        Thread.Sleep(50);
                        this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                    }
                });
        }
    }
}

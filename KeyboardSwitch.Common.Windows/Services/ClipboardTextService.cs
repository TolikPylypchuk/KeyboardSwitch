using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Common.Windows.Services
{
    public class ClipboardTextService : ITextService
    {
        private readonly IInputSimulator input;
        private readonly ILogger<ClipboardTextService> logger;

        public ClipboardTextService(IInputSimulator input, ILogger<ClipboardTextService> logger)
        {
            this.input = input;
            this.logger = logger;
        }

        public Task<string?> GetTextAsync()
        {
            this.logger.LogTrace("Getting the text from the clipboard.");

            return TaskUtils.RunSTATask(() =>
            {
                this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                Thread.Sleep(50);
                return Clipboard.ContainsText() ? Clipboard.GetText() : null;
            });
        }

        public Task SetTextAsync(string text)
        {
            this.logger.LogTrace("Setting the text into the clipboard.");

            return TaskUtils.RunSTATask(() =>
            {
                Clipboard.SetText(text);
                Thread.Sleep(50);
                this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            });
        }
    }
}

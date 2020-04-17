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
        private readonly ILogger<ClipboardTextService> logger;

        public ClipboardTextService(IInputSimulator input, ILogger<ClipboardTextService> logger)
        {
            this.input = input;
            this.logger = logger;
        }

        public Task<string?> GetTextAsync()
        {
            this.logger.LogTrace("Simulating pressing Ctrl+C and getting the text from the clipboard");

            return TaskUtils.RunSTATask(() =>
            {
                this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                Thread.Sleep(50);
                return Clipboard.ContainsText() ? Clipboard.GetText() : null;
            });
        }

        public Task SetTextAsync(string text)
        {
            this.logger.LogTrace("Setting the text into the clipboard and simulating pressing Ctrl+V");

            return TaskUtils.RunSTATask(() =>
            {
                Clipboard.SetText(text);
                Thread.Sleep(50);
                this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
            });
        }
    }
}

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;

using KeyboardSwitch.Common.Services;

using Microsoft.Extensions.Logging;

using Vanara.PInvoke;

using static Vanara.PInvoke.Kernel32;
using static Vanara.PInvoke.User32;

namespace KeyboardSwitch.Windows.Services
{
    internal class ClipboardTextService : ITextService
    {
        private readonly IInputSimulator input;
        private readonly IAppSettingsService settingsService;
        private readonly ILogger<ClipboardTextService> logger;

        private string? savedClipboardText;

        public ClipboardTextService(
            IInputSimulator input,
            IAppSettingsService settingsService,
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
                        this.savedClipboardText = this.GetClipboardText();
                        this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                        Thread.Sleep(50);
                    }

                    return this.GetClipboardText();
                });
        }

        public async Task SetTextAsync(string text)
        {
            this.logger.LogDebug("Setting the text into the clipboard");

            var settings = await this.settingsService.GetAppSettingsAsync();

            await TaskUtils.RunSTATask(() =>
                {
                    SetClipboardText(text);

                    if (settings.InstantSwitching)
                    {
                        Thread.Sleep(50);
                        this.input.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

                        if (this.savedClipboardText != null)
                        {
                            var text = this.savedClipboardText;
                            this.savedClipboardText = null;
                            Thread.Sleep(50);
                            this.SetClipboardText(text);
                        }
                    }
                });
        }

        private string? GetClipboardText()
        {
            if (!IsClipboardFormatAvailable(CLIPFORMAT.CF_UNICODETEXT))
            {
                return null;
            }

            try
            {
                if (!OpenClipboard(IntPtr.Zero))
                {
                    return null;
                }

                var handle = GetClipboardData(CLIPFORMAT.CF_UNICODETEXT);
                var pointer = IntPtr.Zero;

                try
                {
                    pointer = GlobalLock(handle);

                    if (pointer == IntPtr.Zero)
                    {
                        return null;
                    }

                    int size = GlobalSize(handle);
                    var buffer = new byte[size];

                    Marshal.Copy(pointer, buffer, 0, size);

                    return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
                } finally
                {
                    if (pointer != IntPtr.Zero)
                    {
                        GlobalUnlock(handle);
                    }
                }
            } finally
            {
                CloseClipboard();
            }
        }

        private void SetClipboardText(string text)
        {
            try
            {
                if (!OpenClipboard(IntPtr.Zero))
                {
                    return;
                }

                var handle = Marshal.StringToHGlobalUni(text);

                var pointer = IntPtr.Zero;

                try
                {
                    pointer = GlobalLock(handle);

                    if (pointer == IntPtr.Zero)
                    {
                        return;
                    }

                    SetClipboardData(CLIPFORMAT.CF_UNICODETEXT, handle);
                } finally
                {
                    if (pointer != IntPtr.Zero)
                    {
                        GlobalUnlock(handle);
                    }
                }
            } finally
            {
                CloseClipboard();
            }
        }
    }
}

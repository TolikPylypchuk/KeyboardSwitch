namespace KeyboardSwitch.Windows.Services;

using System.Reactive.Disposables;
using System.Runtime.InteropServices;

internal sealed class WinClipboardService(ILogger<WinClipboardService> logger) : IClipboardService
{
    private const int RetryCount = 10;
    private const int Delay = 100;

    public async Task<string?> GetTextAsync()
    {
        logger.LogDebug("Getting text from the clipboard");

        using (await this.OpenClipboardAsync())
        {
            var hText = User32.GetClipboardData(CLIPFORMAT.CF_UNICODETEXT);
            if (hText == IntPtr.Zero)
            {
                return null;
            }

            var pText = Kernel32.GlobalLock(hText);
            if (pText == IntPtr.Zero)
            {
                return null;
            }

            var result = Marshal.PtrToStringUni(pText);
            Kernel32.GlobalUnlock(hText);
            return result;
        }
    }

    public async Task SetTextAsync(string text)
    {
        logger.LogDebug("Setting text into the clipboard");

        using (await this.OpenClipboardAsync())
        {
            User32.EmptyClipboard();

            if (text is not null)
            {
                var hGlobal = Marshal.StringToHGlobalUni(text);
                User32.SetClipboardData(CLIPFORMAT.CF_UNICODETEXT, hGlobal);
            }
        }
    }

    private async Task<IDisposable> OpenClipboardAsync()
    {
        int i = 0;

        while (!User32.OpenClipboard(IntPtr.Zero))
        {
            if (++i == RetryCount)
            {
                throw new TimeoutException("Timeout when opening the clipboard");
            }

            await Task.Delay(Delay);
        }

        return Disposable.Create(() => User32.CloseClipboard());
    }
}

namespace KeyboardSwitch.Windows.Services;

using System.Reactive.Disposables;
using System.Runtime.InteropServices;

internal class WinClipboardService : IClipboardService
{
    private const int RetryCount = 10;
    private const int Delay = 100;

    public async Task<string?> GetTextAsync()
    {
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
                throw new TimeoutException("Timeout opening clipboard");
            }

            await Task.Delay(Delay);
        }

        return Disposable.Create(() => User32.CloseClipboard());
    }
}

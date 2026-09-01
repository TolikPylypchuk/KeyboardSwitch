using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;

namespace KeyboardSwitch.Windows.Services;

internal sealed partial class WinClipboardService(IScheduler scheduler, ILogger<WinClipboardService> logger)
    : ClipboardServiceBase(scheduler)
{
    private const int RetryCount = 10;
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(100);

    public override async Task<string?> GetText()
    {
        this.LogGettingTextFromClipboard();

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

    public override async Task SetText(string text)
    {
        this.LogSettingTextIntoClipboard();

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

            await this.Scheduler.Sleep(Delay);
        }

        return Disposable.Create(() => User32.CloseClipboard());
    }

    [LoggerMessage(LogLevel.Debug, "Getting text from the clipboard")]
    private partial void LogGettingTextFromClipboard();

    [LoggerMessage(LogLevel.Debug, "Setting text into the clipboard")]
    private partial void LogSettingTextIntoClipboard();
}

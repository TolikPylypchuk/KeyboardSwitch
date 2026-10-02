using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;

namespace KeyboardSwitch.Windows.Services;

internal sealed partial class WinClipboardService(IScheduler scheduler, ILogger<WinClipboardService> logger)
    : ClipboardServiceBase(scheduler)
{
    private const int RetryCount = 10;
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(100);

    private static readonly uint ExcludeFromMonitorProcessingFormat =
        User32.RegisterClipboardFormat("ExcludeClipboardContentFromMonitorProcessing");

    private static readonly uint CanIncludeInClipboardHistoryFormat =
        User32.RegisterClipboardFormat("CanIncludeInClipboardHistory");

    private static readonly uint CanUploadToCloudClipboardFormat =
        User32.RegisterClipboardFormat("CanUploadToCloudClipboard");

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

    public override async Task SetText(string text, bool excludeFromHistory)
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

            if (excludeFromHistory)
            {
                this.LogExcludingFromClipboardHistory();

                SetClipboardFormat(ExcludeFromMonitorProcessingFormat);
                SetClipboardFormat(CanIncludeInClipboardHistoryFormat);
                SetClipboardFormat(CanUploadToCloudClipboardFormat);
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

    private static void SetClipboardFormat(uint format)
    {
        if (format == 0)
        {
            return;
        }

        var hGlobal = Kernel32.GlobalAlloc(Kernel32.GMEM.GMEM_MOVEABLE, sizeof(int));
        if (hGlobal.IsNull)
        {
            return;
        }

        var pValue = Kernel32.GlobalLock(hGlobal);
        if (pValue == IntPtr.Zero)
        {
            Kernel32.GlobalFree(hGlobal);
            return;
        }

        Marshal.WriteInt32(pValue, 0);
        Kernel32.GlobalUnlock(hGlobal);

        if (User32.SetClipboardData(format, hGlobal.DangerousGetHandle()) == IntPtr.Zero)
        {
            Kernel32.GlobalFree(hGlobal);
        }
    }

    [LoggerMessage(LogLevel.Debug, "Getting text from the clipboard")]
    private partial void LogGettingTextFromClipboard();

    [LoggerMessage(LogLevel.Debug, "Setting text into the clipboard")]
    private partial void LogSettingTextIntoClipboard();

    [LoggerMessage(LogLevel.Debug, "Excluding the text from the clipboard history")]
    private partial void LogExcludingFromClipboardHistory();
}

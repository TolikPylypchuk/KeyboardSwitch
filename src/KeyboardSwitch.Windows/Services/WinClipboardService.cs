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
        PInvoke.RegisterClipboardFormat("ExcludeClipboardContentFromMonitorProcessing");

    private static readonly uint CanIncludeInClipboardHistoryFormat =
        PInvoke.RegisterClipboardFormat("CanIncludeInClipboardHistory");

    private static readonly uint CanUploadToCloudClipboardFormat =
        PInvoke.RegisterClipboardFormat("CanUploadToCloudClipboard");

    public override async Task<string?> GetText()
    {
        this.LogGettingTextFromClipboard();

        using (await this.OpenClipboardAsync())
        {
            return GetClipboardText();
        }
    }

    public override async Task SetText(string text, bool excludeFromHistory)
    {
        this.LogSettingTextIntoClipboard();

        using (await this.OpenClipboardAsync())
        {
            PInvoke.EmptyClipboard();

            if (text is not null)
            {
                var hGlobal = Marshal.StringToHGlobalUni(text);
                PInvoke.SetClipboardData((uint)CLIPBOARD_FORMAT.CF_UNICODETEXT, (HANDLE)hGlobal);
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

        while (!PInvoke.OpenClipboard(HWND.Null))
        {
            if (++i == RetryCount)
            {
                throw new TimeoutException("Timeout when opening the clipboard");
            }

            await this.Scheduler.Sleep(Delay);
        }

        return Disposable.Create(() => PInvoke.CloseClipboard());
    }

    private static unsafe string? GetClipboardText()
    {
        var hText = PInvoke.GetClipboardData((uint)CLIPBOARD_FORMAT.CF_UNICODETEXT);
        if (hText.IsNull)
        {
            return null;
        }

        var hGlobal = (HGLOBAL)hText.Value;

        var pText = PInvoke.GlobalLock(hGlobal);
        if (pText is null)
        {
            return null;
        }

        var result = Marshal.PtrToStringUni((nint)pText);
        PInvoke.GlobalUnlock(hGlobal);
        return result;
    }

    private static unsafe void SetClipboardFormat(uint format)
    {
        if (format == 0)
        {
            return;
        }

        var hGlobal = PInvoke.GlobalAlloc(GLOBAL_ALLOC_FLAGS.GMEM_MOVEABLE, sizeof(int));
        if (hGlobal.IsNull)
        {
            return;
        }

        var pValue = PInvoke.GlobalLock(hGlobal);
        if (pValue is null)
        {
            PInvoke.GlobalFree(hGlobal);
            return;
        }

        *(int*)pValue = 0;
        PInvoke.GlobalUnlock(hGlobal);

        if (PInvoke.SetClipboardData(format, (HANDLE)hGlobal.Value).IsNull)
        {
            PInvoke.GlobalFree(hGlobal);
        }
    }

    [LoggerMessage(LogLevel.Debug, "Getting text from the clipboard")]
    private partial void LogGettingTextFromClipboard();

    [LoggerMessage(LogLevel.Debug, "Setting text into the clipboard")]
    private partial void LogSettingTextIntoClipboard();

    [LoggerMessage(LogLevel.Debug, "Excluding the text from the clipboard history")]
    private partial void LogExcludingFromClipboardHistory();
}

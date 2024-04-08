using System.Reactive.Concurrency;

using static KeyboardSwitch.Core.Constants;

namespace KeyboardSwitch.MacOS.Services;

internal sealed class MacClipboardService(IScheduler scheduler, ILogger<MacClipboardService> logger) : IClipboardService
{
    private static readonly IntPtr NSString = AppKit.GetClass("NSString");
    private static readonly IntPtr NSPasteboard = AppKit.GetClass("NSPasteboard");

    private static readonly IntPtr NSStringPboardType;
    private static readonly IntPtr UtfTextType;
    private static readonly IntPtr GeneralPasteboard;

    private static readonly IntPtr InitWithUtf8 = AppKit.RegisterName("initWithUTF8String:");
    private static readonly IntPtr Alloc = AppKit.RegisterName("alloc");
    private static readonly IntPtr Release = AppKit.RegisterName("release");
    private static readonly IntPtr SetString = AppKit.RegisterName("setString:forType:");
    private static readonly IntPtr StringForType = AppKit.RegisterName("stringForType:");
    private static readonly IntPtr Utf8String = AppKit.RegisterName("UTF8String");
    private static readonly IntPtr ClearContents = AppKit.RegisterName("clearContents");

    static MacClipboardService()
    {
        UtfTextType = AppKit.SendMessage(
            AppKit.SendMessage(NSString, Alloc), InitWithUtf8, "public.utf8-plain-text");
        NSStringPboardType = AppKit.SendMessage(
            AppKit.SendMessage(NSString, Alloc), InitWithUtf8, "NSStringPboardType");

        GeneralPasteboard = AppKit.SendMessage(NSPasteboard, AppKit.RegisterName("generalPasteboard"));
    }

    public Task<string?> GetText()
    {
        logger.LogDebug("Getting text from the clipboard");

        var ptr = AppKit.SendMessage(GeneralPasteboard, StringForType, NSStringPboardType);
        var charArray = AppKit.SendMessage(ptr, Utf8String);
        var result = Marshal.PtrToStringAnsi(charArray);

        return Task.FromResult(result);
    }

    public Task SetText(string text)
    {
        logger.LogDebug("Setting text into the clipboard");

        IntPtr str = default;
        try
        {
            var nsString = AppKit.SendMessage(NSString, Alloc);
            str = AppKit.SendMessage(nsString, InitWithUtf8, text);
            AppKit.SendMessage(GeneralPasteboard, ClearContents);
            AppKit.SendMessage(GeneralPasteboard, SetString, str, UtfTextType);
        } finally
        {
            if (str != default)
            {
                AppKit.SendMessage(str, Release);
            }
        }

        return Task.CompletedTask;
    }

    public async Task<IAsyncDisposable> SaveClipboardState()
    {
        var savedText = await this.GetText();
        var saveTime = scheduler.Now;

        return new AsyncDisposable(async () =>
        {
            if (!String.IsNullOrEmpty(savedText) && scheduler.Now - saveTime < MaxClipboardRestoreDuration)
            {
                await scheduler.Sleep(TimeSpan.FromMilliseconds(50));
                await this.SetText(savedText);
            }
        });
    }
}

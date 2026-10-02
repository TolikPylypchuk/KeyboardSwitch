namespace KeyboardSwitch.Core.Services.Clipboard;

public interface IClipboardService
{
    Task<string?> GetText();

    Task SetText(string text, bool excludeFromHistory);

    Task<IAsyncDisposable> SaveClipboardState();
}

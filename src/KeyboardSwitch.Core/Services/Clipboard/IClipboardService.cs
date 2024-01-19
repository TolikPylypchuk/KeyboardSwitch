namespace KeyboardSwitch.Core.Services.Clipboard;

public interface IClipboardService
{
    Task<string?> GetTextAsync();
    Task SetTextAsync(string text);
}

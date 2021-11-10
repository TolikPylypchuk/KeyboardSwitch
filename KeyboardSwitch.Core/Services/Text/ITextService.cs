namespace KeyboardSwitch.Core.Services.Text;

public interface ITextService
{
    Task<string?> GetTextAsync();
    Task SetTextAsync(string text);
}

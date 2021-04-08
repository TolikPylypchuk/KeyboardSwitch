using System.Threading.Tasks;

namespace KeyboardSwitch.Core.Services
{
    public interface ITextService
    {
        Task<string?> GetTextAsync();
        Task SetTextAsync(string text);
    }
}

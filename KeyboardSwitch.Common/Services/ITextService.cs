using System.Threading.Tasks;

namespace KeyboardSwitch.Common.Services
{
    public interface ITextService
    {
        Task<string?> GetTextAsync();
        Task SetTextAsync(string text);
    }
}

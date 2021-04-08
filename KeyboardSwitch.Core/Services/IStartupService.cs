using System.Threading.Tasks;

namespace KeyboardSwitch.Core.Services
{
    public interface IStartupService
    {
        bool IsStartupConfigured();
        Task ConfigureStartupAsync(bool startup);
    }
}

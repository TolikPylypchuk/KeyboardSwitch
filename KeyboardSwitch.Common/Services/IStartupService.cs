using System.Threading.Tasks;

namespace KeyboardSwitch.Common.Services
{
    public interface IStartupService
    {
        bool IsStartupConfigured();
        Task ConfigureStartupAsync(bool startup);
    }
}

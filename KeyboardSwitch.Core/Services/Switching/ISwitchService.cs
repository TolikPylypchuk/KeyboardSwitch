using System.Threading.Tasks;

namespace KeyboardSwitch.Core.Services.Switching
{
    public interface ISwitchService
    {
        Task SwitchTextAsync(SwitchDirection direction);
    }
}

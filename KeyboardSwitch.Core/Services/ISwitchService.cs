using System.Threading.Tasks;

namespace KeyboardSwitch.Core.Services
{
    public enum SwitchDirection
    {
        Forward,
        Backward
    }

    public interface ISwitchService
    {
        Task SwitchTextAsync(SwitchDirection direction);
    }
}

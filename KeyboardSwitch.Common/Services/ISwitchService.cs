using System.Threading.Tasks;

namespace KeyboardSwitch.Common.Services
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

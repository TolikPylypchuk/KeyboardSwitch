using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

namespace KeyboardSwitch.Common.Windows.Services
{
    public class SwitchService : ISwitchService
    {
        public Task SwitchTextAsync(SwitchDirection direction)
        {
            return Task.CompletedTask;
        }
    }
}

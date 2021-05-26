using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using KeyboardSwitch.Core.Services.Settings;

namespace KeyboardSwitch.Core.Services.Infrastructure
{
    public sealed class DirectServiceCommunicator : IServiceCommunicator
    {
        private readonly IAppSettingsService settingsService;
        private readonly INamedPipeService namedPipeService;

        public DirectServiceCommunicator(
            IAppSettingsService settingsService,
            ServiceProvider<INamedPipeService> namedPipeServiceProvider)
        {
            this.settingsService = settingsService;
            this.namedPipeService = namedPipeServiceProvider(nameof(KeyboardSwitch));
        }

        public bool IsServiceRunning() =>
            Process.GetProcessesByName(nameof(KeyboardSwitch)).Length > 0;

        public async Task StartServiceAsync()
        {
            var settings = await settingsService.GetAppSettingsAsync();
            Process.Start(settings.ServicePath);
        }

        public void StopService(bool kill)
        {
            if (kill)
            {
                Process.GetProcessesByName(nameof(KeyboardSwitch)).ForEach(process => process.Kill());
            } else
            {
                this.namedPipeService.Write(ExternalCommand.Stop);
            }
        }

        public void ReloadService() =>
            this.namedPipeService.Write(ExternalCommand.ReloadSettings);
    }
}

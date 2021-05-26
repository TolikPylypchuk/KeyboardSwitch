using System.Threading.Tasks;

using KeyboardSwitch.Core.Services.Infrastructure;
using KeyboardSwitch.Core.Settings;

using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Linux.Services
{
    internal sealed class SystemdServiceCommunicator : IServiceCommunicator
    {
        private readonly IServiceCommunicator fallbackCommunicator;
        private readonly string serviceName;

        public SystemdServiceCommunicator(
            IServiceCommunicator fallbackCommunicator,
            IOptions<GlobalSettings> globalSettings)
        {
            this.fallbackCommunicator = fallbackCommunicator;
            this.serviceName = globalSettings.Value.SystemdService;
        }

        public bool IsServiceRunning() =>
            Systemd.IsLoaded(this.serviceName)
                ? Systemd.IsActive(this.serviceName)
                : this.fallbackCommunicator.IsServiceRunning();

        public async Task StartServiceAsync()
        {
            if (Systemd.IsLoaded(this.serviceName))
            {
                Systemd.Start(this.serviceName);
            } else
            {
                await this.fallbackCommunicator.StartServiceAsync();
            }
        }

        public void ReloadService()
        {
            if (Systemd.IsLoaded(this.serviceName))
            {
                Systemd.Reload(this.serviceName);
            } else
            {
                this.fallbackCommunicator.ReloadService();
            }
        }

        public void StopService(bool kill)
        {
            if (Systemd.IsLoaded(this.serviceName))
            {
                if (kill)
                {
                    Systemd.Kill(this.serviceName);
                } else
                {
                    Systemd.Stop(this.serviceName);
                }
            } else
            {
                this.fallbackCommunicator.StopService(kill);
            }
        }
    }
}

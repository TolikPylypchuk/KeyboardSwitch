using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Core.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Linux.Services
{
    public sealed class SystemdStartupService : IStartupService
    {
        private readonly string ServiceName;
        private readonly ILogger<SystemdStartupService> logger;

        public SystemdStartupService(IOptions<GlobalSettings> settings, ILogger<SystemdStartupService> logger)
        {
            this.ServiceName = settings.Value.SystemdService;
            this.logger = logger;
        }

        public bool IsStartupConfigured(AppSettings settings)
        {
            this.logger.LogDebug("Checking if the KeyboardSwitch service is configured to run on startup");

            bool isConfigured = Systemd.IsEnabled(this.ServiceName);

            this.logger.LogDebug($"KeyboardSwitch {(isConfigured ? "is" : "is not")} configured to run on startup");

            return isConfigured;
        }

        public void ConfigureStartup(AppSettings settings, bool startup)
        {
            this.logger.LogDebug(
                $"Configuring to {(startup ? "start" : "stop")} running the KeyboardSwitch service on startup");

            if (startup)
            {
                Systemd.Enable(this.ServiceName);
            } else
            {
                Systemd.Disable(this.ServiceName);
            }

            this.logger.LogDebug(
                $"Configured to {(startup ? "start" : "stop")} running the KeyboardSwitch service on startup");
        }
    }
}

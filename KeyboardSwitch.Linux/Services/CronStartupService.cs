using System;
using System.IO;
using System.Linq;

using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Core.Settings;

using Microsoft.Extensions.Logging;

namespace KeyboardSwitch.Linux.Services
{
    public sealed class CronStartupService : IStartupService
    {
        private readonly ILogger<CronStartupService> logger;

        public CronStartupService(ILogger<CronStartupService> logger) =>
            this.logger = logger;

        public bool IsStartupConfigured(AppSettings settings)
        {
            this.logger.LogDebug("Checking if the KeyboardSwitch service is configured to run on startup");

            var result = Bash.Run("crontab -l");

            if (result.ExitCode != 0)
            {
                this.logger.LogError($"crontab returned an error: {result.ErrorOutput}. " +
                    "This is probably because you forgot to configure crontab. " +
                    "You can do that by running 'crontab -e'");

                return false;
            }

            bool isConfigured = result.Output
                .Split(Environment.NewLine)
                .Where(cronLine => !cronLine.StartsWith("#"))
                .Any(cronLine => cronLine.Contains(settings.ServicePath));

            this.logger.LogDebug($"KeyboardSwitch {(isConfigured ? "is" : "is not")} configured to run on startup");

            return isConfigured;
        }

        public void ConfigureStartup(AppSettings settings, bool startup)
        {
            this.logger.LogDebug(
                $"Configuring to {(startup ? "start" : "stop")} running the KeyboardSwitch service on startup");

            string command = startup
                ? $"(crontab -l; echo '{this.GetCronExpression(settings)}'; echo '') | crontab -"
                : $"crontab -l | grep -v '{settings.ServicePath}' | crontab -";

            var result = Bash.Run(command);

            if (result.ExitCode != 0)
            {
                this.logger.LogError($"crontab returned an error: {result.ErrorOutput}. " +
                    "This is probably because you forgot to configure crontab. " +
                    "You can do that by running 'crontab -e'");
            } else
            {
                this.logger.LogDebug(
                    $"Configured to {(startup ? "start" : "stop")} running the KeyboardSwitch service on startup");
            }
        }

        private string GetCronExpression(AppSettings settings) =>
            $"@reboot {Path.GetFullPath(settings.ServicePath)}";
    }
}

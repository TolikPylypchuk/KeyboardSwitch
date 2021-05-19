using System;
using System.IO;

using KeyboardSwitch.Core.Services;
using KeyboardSwitch.Core.Settings;

using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace KeyboardSwitch.Windows.Services
{
    internal class RegistryStartupService : IStartupService
    {
        private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string StartupRegistryName = "Keyboard Switch";
        private const string ExecutableExtension = ".exe";

        private readonly ILogger<RegistryStartupService> logger;

        public RegistryStartupService(ILogger<RegistryStartupService> logger) =>
            this.logger = logger;

        public bool IsStartupConfigured(AppSettings settings)
        {
            this.logger.LogDebug("Checking if the KeyboardSwitch service is configured to run on startup");

            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey);
            bool isConfigured = key?.GetValue(StartupRegistryName) != null;

            this.logger.LogDebug($"KeyboardSwitch {(isConfigured ? "is" : "is not")} configured to run on startup");

            return isConfigured;
        }

        public void ConfigureStartup(AppSettings settings, bool startup)
        {
            this.logger.LogDebug(
                $"Configuring to {(startup ? "start" : "stop")} running the KeyboardSwitch service on startup");

            using var startupKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);

            if (startup)
            {
                startupKey?.SetValue(StartupRegistryName, this.GetServicePath(settings), RegistryValueKind.String);
            } else
            {
                startupKey?.DeleteValue(StartupRegistryName);
            }

            this.logger.LogDebug(
                $"Configured to {(startup ? "start" : "stop")} running the KeyboardSwitch service on startup");
        }

        private string GetServicePath(AppSettings settings)
        {
            var path = settings.ServicePath.EndsWith(ExecutableExtension, StringComparison.InvariantCultureIgnoreCase)
                ? settings.ServicePath
                : settings.ServicePath + ExecutableExtension;

            return $"\"{Path.GetFullPath(path)}\"";
        }
    }
}

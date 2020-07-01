using System;
using System.IO;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

using Microsoft.Win32;

namespace KeyboardSwitch.Windows.Services
{
    public class RegistryStartupService : IStartupService
    {
        private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string StartupRegistryName = "Keyboard Switch";
        private const string ExecutableExtension = ".exe";

        private readonly IAppSettingsService appSettingsService;

        public RegistryStartupService(IAppSettingsService appSettingsService)
            => this.appSettingsService = appSettingsService;

        public bool IsStartupConfigured()
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey);
            return key.GetValue(StartupRegistryName) != null;
        }

        public async Task ConfigureStartupAsync(bool startup)
        {
            using var startupKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);

            if (startup)
            {
                startupKey.SetValue(StartupRegistryName, await this.GetServicePath(), RegistryValueKind.String);
            } else
            {
                startupKey.DeleteValue(StartupRegistryName);
            }
        }

        private async Task<string> GetServicePath()
        {
            var settings = await this.appSettingsService.GetAppSettingsAsync();
            var path = settings.ServicePath.EndsWith(ExecutableExtension, StringComparison.InvariantCultureIgnoreCase)
                ? settings.ServicePath
                : settings.ServicePath + ExecutableExtension;

            return $"\"{Path.GetFullPath(path)}\"";
        }
    }
}

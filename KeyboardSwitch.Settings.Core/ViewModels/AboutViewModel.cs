using System;
using System.Diagnostics;
using System.Net;
using System.Reactive;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

using static KeyboardSwitch.Settings.Core.Constants;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public class AboutViewModel : ReactiveObject
    {
        public AboutViewModel()
        {
            this.AppVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            this.CheckForUpdates = ReactiveCommand.CreateFromTask(this.OnCheckForUpdates);
            this.GetNewVersion = ReactiveCommand.Create(this.OnGetNewVersion);

            this.CheckForUpdates.ToPropertyEx(this, vm => vm.LatestVersion, initialValue: this.AppVersion);
        }

        public Version AppVersion { get; }
        public Version LatestVersion { [ObservableAsProperty] get; } = null!;

        public ReactiveCommand<Unit, Version> CheckForUpdates { get; }
        public ReactiveCommand<Unit, Unit> GetNewVersion { get; }

        public async Task<Version> OnCheckForUpdates()
        {
            try
            {
                using var webClient = new WebClient();
                string version = await Task.Run(() => webClient.DownloadStringTaskAsync(new Uri(VersionInfoLocation)));
                return Version.Parse(version.Trim());
            } catch (Exception e)
            {
                this.Log().Error(e, "Cannot get the latest version info when checking for updates");
                return this.AppVersion;
            }
        }

        public void OnGetNewVersion()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo { FileName = AppReleasesLocation, UseShellExecute = true });
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", AppReleasesLocation);
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", AppReleasesLocation);
            }
        }
    }
}

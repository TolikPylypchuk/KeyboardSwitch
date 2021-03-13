using System;
using System.Globalization;
using System.Net;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;

using KeyboardSwitch.Common;

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
            this.OpenDocs = ReactiveCommand.Create(this.OnOpenDocs);

            this.CheckForUpdates.ToPropertyEx(this, vm => vm.LatestVersion, initialValue: this.AppVersion);
        }

        public Version AppVersion { get; }
        public Version LatestVersion { [ObservableAsProperty] get; } = null!;

        public ReactiveCommand<Unit, Version> CheckForUpdates { get; }
        public ReactiveCommand<Unit, Unit> GetNewVersion { get; }
        public ReactiveCommand<Unit, Unit> OpenDocs { get; }

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

        public void OnGetNewVersion() =>
            new Uri(AppReleasesLocation).OpenInBrowser();

        public void OnOpenDocs() =>
            new Uri(DocsLocation).OpenInBrowser();
    }
}

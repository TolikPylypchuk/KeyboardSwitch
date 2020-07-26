using System;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public class AboutViewModel : ReactiveObject
    {
        public AboutViewModel()
        {
            this.AppVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            this.CheckForUpdates = ReactiveCommand.CreateFromTask(this.OnCheckForUpdates);

            this.CheckForUpdates.ToPropertyEx(this, vm => vm.LatestVersion, initialValue: this.AppVersion);
        }

        public Version AppVersion { get; }

        public Version LatestVersion { [ObservableAsProperty] get; } = null!;

        public ReactiveCommand<Unit, Version> CheckForUpdates { get; }

        public async Task<Version> OnCheckForUpdates()
        {
            using var webClient = new WebClient();
            string content = await Task.Run(() => webClient.DownloadStringTaskAsync(
                new Uri("https://raw.githubusercontent.com/TolikPylypchuk/KeyboardSwitch/master/version-info.json")));
            return JsonSerializer.Deserialize<Version>(content);
        }
    }
}

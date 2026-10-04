using System.Reflection;

namespace KeyboardSwitch.Settings.Core.ViewModels;

public sealed partial class AboutViewModel : ReactiveObject
{
    public AboutViewModel()
    {
        this.AppVersion = Assembly.GetExecutingAssembly().GetName().Version!;

        this._latestVersionHelper = this.CheckForUpdatesCommand
            .ToProperty(this, vm => vm.LatestVersion, initialValue: this.AppVersion);
    }

    public Version AppVersion { get; }

    [ObservableAsProperty]
    public partial Version LatestVersion { get; }

    [ReactiveCommand]
    private async Task<Version> CheckForUpdates()
    {
        try
        {
            using var httpClient = new HttpClient();
            string version = await Task.Run(() => httpClient.GetStringAsync(VersionInfoLocation));
            return Version.Parse(version.Trim());
        } catch (Exception e)
        {
            this.Log().Error(e, "Cannot get the latest version info when checking for updates");
            return this.AppVersion;
        }
    }

    [ReactiveCommand]
    private void GetNewVersion() =>
        new Uri(AppReleasesLocation).OpenInBrowser();

    [ReactiveCommand]
    private void OpenDocs() =>
        new Uri(DocsLocation).OpenInBrowser();
}

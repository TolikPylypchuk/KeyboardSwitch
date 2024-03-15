namespace KeyboardSwitch.Settings.Core.ViewModels;

using System.Reflection;

public class AboutViewModel : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<Version> latestVersion;

    public AboutViewModel()
    {
        this.AppVersion = Assembly.GetExecutingAssembly().GetName().Version!;
        this.CheckForUpdates = ReactiveCommand.CreateFromTask(this.OnCheckForUpdates);
        this.GetNewVersion = ReactiveCommand.Create(this.OnGetNewVersion);
        this.OpenDocs = ReactiveCommand.Create(this.OnOpenDocs);

        this.latestVersion = this.CheckForUpdates
            .ToProperty(this, vm => vm.LatestVersion, initialValue: this.AppVersion);
    }

    public Version AppVersion { get; }
    public Version LatestVersion => this.latestVersion.Value;

    public ReactiveCommand<Unit, Version> CheckForUpdates { get; }
    public ReactiveCommand<Unit, Unit> GetNewVersion { get; }
    public ReactiveCommand<Unit, Unit> OpenDocs { get; }

    public async Task<Version> OnCheckForUpdates()
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

    public void OnGetNewVersion() =>
        new Uri(AppReleasesLocation).OpenInBrowser();

    public void OnOpenDocs() =>
        new Uri(DocsLocation).OpenInBrowser();
}

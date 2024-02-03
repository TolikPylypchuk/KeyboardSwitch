namespace KeyboardSwitch.Settings.State;

using System.Text.Json;

using Microsoft.Extensions.Options;

internal sealed class JsonSuspensionDriver(IOptions<GlobalSettings> settings) : ISuspensionDriver
{
    private readonly FileInfo file = new(Environment.ExpandEnvironmentVariables(settings.Value.StateFilePath));

    public IObservable<Unit> InvalidateState()
    {
        if (this.file.Exists)
        {
            this.file.Delete();
        }

        return Observable.Return(Unit.Default);
    }

    public IObservable<object> LoadState()
    {
        if (!this.file.Exists)
        {
            return Observable.Return(new AppState());
        }

        return Observable.FromAsync(async () =>
        {
            using var stream = new BufferedStream(this.file.OpenRead());
            var state = await JsonSerializer.DeserializeAsync(stream, AppStateContext.Default.AppState);
            return state ?? new AppState();
        });
    }

    public IObservable<Unit> SaveState(object state)
    {
        if (state is AppState appState)
        {
            return Observable.FromAsync(async () =>
            {
                this.file.Directory?.Create();
                using var stream = new BufferedStream(this.file.OpenWrite());
                await JsonSerializer.SerializeAsync(stream, appState, AppStateContext.Default.AppState);
            });
        }

        return Observable.Throw<Unit>(new InvalidCastException("AppState must be provided to SaveState"));
    }
}

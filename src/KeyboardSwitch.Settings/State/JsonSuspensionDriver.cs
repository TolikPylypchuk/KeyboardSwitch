using System.Text.Json;

using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Settings.State;

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

        using var stream = new BufferedStream(this.file.OpenRead());
        var state = JsonSerializer.Deserialize(stream, AppStateContext.Default.AppState);

        return Observable.Return(state ?? new AppState());
    }

    public IObservable<Unit> SaveState(object state)
    {
        if (state is AppState appState)
        {
            this.file.Directory?.Create();
            using var stream = new BufferedStream(this.file.OpenWrite());
            JsonSerializer.Serialize(stream, appState, AppStateContext.Default.AppState);

            return Observable.Return(Unit.Default);
        }

        return Observable.Throw<Unit>(new InvalidCastException("AppState must be provided to SaveState"));
    }
}

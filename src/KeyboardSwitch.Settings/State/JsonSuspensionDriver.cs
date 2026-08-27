using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using Microsoft.Extensions.Options;

namespace KeyboardSwitch.Settings.State;

internal sealed class JsonSuspensionDriver(IOptions<GlobalSettings> settings) : ISuspensionDriver
{
    private readonly FileInfo file = new(Environment.ExpandEnvironmentVariables(settings.Value.StateFilePath));

    public IObservable<T> LoadState<T>(JsonTypeInfo<T> info)
    {
        var createObject = info.CreateObject
            ?? throw new InvalidOperationException($"Cannot create a default instance of type {info.Type}");

        if (!this.file.Exists)
        {
            return Observable.Return(createObject());
        }

        using var stream = new BufferedStream(this.file.OpenRead());
        var state = JsonSerializer.Deserialize(stream, info);

        return Observable.Return(state ?? createObject());
    }

    public IObservable<Unit> SaveState<T>(T state, JsonTypeInfo<T> info)
    {
        return Observable.FromAsync(async () =>
        {
            this.file.Directory?.Create();
            this.file.Delete();
            using var stream = new BufferedStream(this.file.OpenWrite());
            await JsonSerializer.SerializeAsync(stream, state, info);
        });
    }

    public IObservable<Unit> InvalidateState()
    {
        if (this.file.Exists)
        {
            this.file.Delete();
        }

        return Observable.Return(Unit.Default);
    }

    IObservable<Unit> ISuspensionDriver.SaveState<T>(T state)
    {
        if (state is AppState appState)
        {
            return this.SaveState(appState, AppStateContext.Default.AppState);
        }

        throw new InvalidOperationException($"{nameof(JsonSuspensionDriver)} can only save {nameof(AppState)}");
    }

    IObservable<object> ISuspensionDriver.LoadState()
    {
        if (!this.file.Exists)
        {
            return Observable.Return(new AppState());
        }

        // Not async because otherwise the application window doesn't load

        using var stream = new BufferedStream(this.file.OpenRead());
        var state = JsonSerializer.Deserialize(stream, AppStateContext.Default.AppState);

        return Observable.Return(state ?? new AppState());
    }
}

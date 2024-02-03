namespace KeyboardSwitch.Settings.State;

using System.Text.Json;

using Microsoft.Extensions.Options;

internal sealed class JsonSuspensionDriver(IOptions<GlobalSettings> settings) : ISuspensionDriver
{
    private readonly string file = Environment.ExpandEnvironmentVariables(settings.Value.StateFilePath);

    public IObservable<Unit> InvalidateState()
    {
        if (File.Exists(this.file))
        {
            File.Delete(this.file);
        }

        return Observable.Return(Unit.Default);
    }

    public IObservable<object> LoadState()
    {
        if (!File.Exists(this.file))
        {
            return Observable.Return(new AppState());
        }

        return Observable.FromAsync(() => File.ReadAllTextAsync(this.file))
            .Select(content => JsonSerializer.Deserialize(content, SourceGenerationContext.Default.AppState))
            .Select(state => state ?? new AppState());
    }

    public IObservable<Unit> SaveState(object state)
    {
        if (state is AppState appState)
        {
            var content = JsonSerializer.Serialize(appState, SourceGenerationContext.Default.AppState);
            return Observable.FromAsync(() => File.WriteAllTextAsync(this.file, content));
        }

        return Observable.Throw<Unit>(new InvalidCastException("AppState must be provided to SaveState"));
    }
}

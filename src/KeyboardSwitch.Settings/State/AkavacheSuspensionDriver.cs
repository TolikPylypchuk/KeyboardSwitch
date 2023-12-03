namespace KeyboardSwitch.Settings.State;

using Akavache;

internal sealed class AkavacheSuspensionDriver<TAppState>(IBlobCache cache) : ISuspensionDriver
    where TAppState : class
{
    private const string AppStateKey = "AppState";

    private readonly IBlobCache cache = cache;

    public IObservable<Unit> InvalidateState() =>
        this.cache.InvalidateObject<TAppState>(AppStateKey);

    public IObservable<object> LoadState() =>
        this.cache.GetObject<TAppState>(AppStateKey).WhereNotNull();

    public IObservable<Unit> SaveState(object state) =>
        this.cache.InsertObject(AppStateKey, (TAppState)state);
}

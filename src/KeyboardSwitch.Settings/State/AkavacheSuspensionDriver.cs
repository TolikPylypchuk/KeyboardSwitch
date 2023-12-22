namespace KeyboardSwitch.Settings.State;

using Akavache;

internal sealed class AkavacheSuspensionDriver<TAppState>(IBlobCache cache) : ISuspensionDriver
    where TAppState : class
{
    private const string AppStateKey = "AppState";

    public IObservable<Unit> InvalidateState() =>
        cache.InvalidateObject<TAppState>(AppStateKey);

    public IObservable<object> LoadState() =>
        cache.GetObject<TAppState>(AppStateKey).WhereNotNull();

    public IObservable<Unit> SaveState(object state) =>
        cache.InsertObject(AppStateKey, (TAppState)state);
}

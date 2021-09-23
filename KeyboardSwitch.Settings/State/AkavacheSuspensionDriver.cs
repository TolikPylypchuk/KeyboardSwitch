using System;
using System.Reactive;

using Akavache;

using ReactiveUI;

namespace KeyboardSwitch.Settings.State
{
    internal sealed class AkavacheSuspensionDriver<TAppState> : ISuspensionDriver where TAppState : class
    {
        private const string AppStateKey = "AppState";

        private readonly IBlobCache cache;

        public AkavacheSuspensionDriver(IBlobCache cache) =>
            this.cache = cache;

        public IObservable<Unit> InvalidateState() =>
            this.cache.InvalidateObject<TAppState>(AppStateKey);

        public IObservable<object> LoadState() =>
            this.cache.GetObject<TAppState>(AppStateKey).WhereNotNull();

        public IObservable<Unit> SaveState(object state) =>
            this.cache.InsertObject(AppStateKey, (TAppState)state);
    }
}

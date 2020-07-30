using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Akavache;

namespace KeyboardSwitch.Common
{
    public static class ObservableExtensions
    {
        public static IObservable<Unit> Discard<T>(this IObservable<T> observable)
            => observable.Select(_ => Unit.Default);

        public static IObservable<bool> AllTrue(this IObservable<IEnumerable<bool>> observable)
            => observable.Select(values => values.All(value => value));

        public static IObservable<bool> AnyTrue(this IObservable<IEnumerable<bool>> observable)
            => observable.Select(values => values.Any(value => value));

        public static IObservable<T> WhereNotNull<T>(this IObservable<T?> observable)
            where T : class
            => observable.Where(x => x != null)!;

        public static IObservable<T> WhereValueNotNull<T>(this IObservable<T?> observable)
            where T : struct
            => observable.Where(x => x.HasValue).Select(x => x ?? default);

        public static IObservable<bool> Invert(this IObservable<bool> observable)
            => observable.Select(value => !value);

        public static IDisposable Subscribe(this IObservable<Unit> observable, Action observer)
            => observable.Subscribe(_ => observer());

        public static IDisposable SubscribeAsync<T>(this IObservable<T> observable, Func<T, Task> observer)
            => observable.SelectMany(async x =>
            {
                await observer(x);
                return Unit.Default;
            }).Subscribe();

        public static IDisposable SubscribeAsync(this IObservable<Unit> observable, Func<Task> observer)
            => observable.SelectMany(async _ =>
            {
                await observer();
                return Unit.Default;
            }).Subscribe();

        public static Task<bool> ContainsKey(this IBlobCache cache, string key)
        {
            var completionSource = new TaskCompletionSource<bool>();

            cache.Get(key).Subscribe(
                x => completionSource.SetResult(true),
                ex => completionSource.SetResult(false));

            return completionSource.Task;
        }
    }
}

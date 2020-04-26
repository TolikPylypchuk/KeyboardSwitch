using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Akavache;

namespace KeyboardSwitch.Common
{
    public static class ObservableExtensions
    {
        public static IObservable<bool> Invert(this IObservable<bool> observable)
            => observable.Select(value => !value);

        public static IObservable<Unit> Discard<T>(this IObservable<T> observable)
            => observable.Select(_ => Unit.Default);

        public static void SubscribeAsync<T>(this IObservable<T> observable, Func<T, Task> subscriber)
            => observable.SelectMany(async result =>
            {
                await subscriber(result);
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

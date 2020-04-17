using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace KeyboardSwitch.Common
{
    public static class ObservableExtensions
    {
        public static void SubscribeAsync<T>(this IObservable<T> observable, Func<T, Task> subscriber)
            => observable.SelectMany(async result =>
            {
                await subscriber(result);
                return Unit.Default;
            }).Subscribe();
    }
}

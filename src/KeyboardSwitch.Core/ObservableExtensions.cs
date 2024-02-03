namespace KeyboardSwitch.Core;

public static class ObservableExtensions
{
    public static IObservable<Unit> Discard<T>(this IObservable<T> observable) =>
        observable.Select(_ => Unit.Default);

    public static IObservable<bool> AllTrue(this IObservable<IEnumerable<bool>> observable) =>
        observable.Select(values => values.All(value => value));

    public static IObservable<bool> AnyTrue(this IObservable<IEnumerable<bool>> observable) =>
        observable.Select(values => values.Any(value => value));

    public static IObservable<T> WhereValueNotNull<T>(this IObservable<T?> observable)
        where T : struct =>
        observable.Where(x => x.HasValue).Select(x => x ?? default);

    public static IObservable<bool> Invert(this IObservable<bool> observable) =>
        observable.Select(value => !value);

    public static IDisposable Subscribe(this IObservable<Unit> observable, Action observer) =>
        observable.Subscribe(_ => observer());

    public static IDisposable SubscribeAsync<T>(this IObservable<T> observable, Func<T, Task> observer) =>
        observable.SelectMany(async x =>
        {
            await observer(x);
            return Unit.Default;
        }).Subscribe();

    public static IDisposable SubscribeAsync(this IObservable<Unit> observable, Func<Task> observer) =>
        observable.SelectMany(async _ =>
        {
            await observer();
            return Unit.Default;
        }).Subscribe();
}

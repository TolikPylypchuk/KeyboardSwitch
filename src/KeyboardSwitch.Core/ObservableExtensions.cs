namespace KeyboardSwitch.Core;

public static class ObservableExtensions
{
    extension<T>(IObservable<T> observable)
    {
        public IObservable<Unit> Discard() =>
            observable.Select(_ => Unit.Default);

        public IDisposable SubscribeAsync(Func<T, Task> observer) =>
            observable.SelectMany(async x =>
            {
                await observer(x);
                return Unit.Default;
            }).Subscribe();
    }

    extension(IObservable<IEnumerable<bool>> observable)
    {
        public IObservable<bool> AllTrue() =>
            observable.Select(values => values.All(value => value));

        public IObservable<bool> AnyTrue() =>
            observable.Select(values => values.Any(value => value));
    }

    extension<T>(IObservable<T?> observable)
        where T : struct
    {
        public IObservable<T> WhereValueNotNull() =>
            observable.Where(x => x.HasValue).Select(x => x ?? default);
    }

    extension(IObservable<bool> observable)
    {
        public IObservable<bool> Invert() =>
            observable.Select(value => !value);
    }

    extension(IObservable<Unit> observable)
    {
        public IDisposable Subscribe(Action observer) =>
            observable.Subscribe(_ => observer());

        public IDisposable SubscribeAsync(Func<Task> observer) =>
            observable.SelectMany(async _ =>
            {
                await observer();
                return Unit.Default;
            }).Subscribe();
    }
}

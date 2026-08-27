namespace KeyboardSwitch.Settings.Core;

public static class ServiceExtensions
{
    extension(IReadonlyDependencyResolver resolver)
    {
        public T GetRequiredService<T>() =>
            resolver.GetService<T>() ?? throw new InvalidOperationException($"{typeof(T).FullName} not found");
    }
}

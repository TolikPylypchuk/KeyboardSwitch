namespace KeyboardSwitch.Settings.Core;

public static class ServiceUtil
{
    public static T GetRequiredService<T>() =>
        Locator.Current.GetService<T>() ??
            throw new InvalidOperationException($"{typeof(T).FullName} not found");
}

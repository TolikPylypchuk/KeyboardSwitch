namespace KeyboardSwitch.Retrying;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRetryManager(this IServiceCollection services, IConfiguration config)
    {
        string? retryValue = config["retry"];
        var delays = ParseRetryDelays(retryValue);

        return delays.Count == 0
            ? services.AddSingleton<IRetryManager, NoOpRetryManager>()
            : services.AddSingleton<IRetryManager>(services => new DelayedRetryManager(
                delays, services.GetRequiredService<ILoggerFactory>().CreateLogger<DelayedRetryManager>()));
    }

    private static IImmutableList<TimeSpan> ParseRetryDelays(string? retryValue) =>
        String.IsNullOrWhiteSpace(retryValue)
            ? ImmutableList.Create<TimeSpan>()
            : retryValue
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ParseDelay)
                .Where(delay => delay.HasValue)
                .Select(delay => delay!.Value)
                .ToImmutableList();

    private static TimeSpan? ParseDelay(string delay)
    {
        if (delay.EndsWith("ms") && Int32.TryParse(delay[0..^2], out int milliseconds))
        {
            return TimeSpan.FromMilliseconds(milliseconds);
        } else if (delay.EndsWith("s") && Int32.TryParse(delay[0..^1], out int seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        } else if (delay.EndsWith("m") && Int32.TryParse(delay[0..^1], out int minutes))
        {
            return TimeSpan.FromMinutes(minutes);
        }

        return null;
    }
}

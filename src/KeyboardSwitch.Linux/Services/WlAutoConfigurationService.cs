namespace KeyboardSwitch.Linux.Services;

internal sealed class WlAutoConfigurationService : IAutoConfigurationService
{
    public IReadOnlyDictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts) =>
        ImmutableDictionary<string, string>.Empty;
}

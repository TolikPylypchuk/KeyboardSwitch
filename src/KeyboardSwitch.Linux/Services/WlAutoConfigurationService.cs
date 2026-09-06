namespace KeyboardSwitch.Linux.Services;

internal sealed class WlAutoConfigurationService : IAutoConfigurationService
{
    public IReadOnlyDictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts) =>
        layouts.ToImmutableDictionary(layout => layout.Id, _ => String.Empty);
}

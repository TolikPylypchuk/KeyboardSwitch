namespace KeyboardSwitch.Core.Services.AutoConfiguration;

public interface IAutoConfigurationService
{
    IReadOnlyDictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts);
}

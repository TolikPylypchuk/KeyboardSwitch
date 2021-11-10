namespace KeyboardSwitch.Core.Services.AutoConfiguration;

public interface IAutoConfigurationService
{
    Dictionary<string, string> CreateCharMappings(IEnumerable<KeyboardLayout> layouts);
}

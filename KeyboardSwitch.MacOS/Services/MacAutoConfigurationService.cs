namespace KeyboardSwitch.MacOS.Services;

internal class MacAutoConfigurationService : AutoConfigurationServiceBase
{
    protected override IEnumerable<List<KeyToCharResult>> GetChars(List<string> layoutIds) =>
        Enumerable.Empty<List<KeyToCharResult>>();
}

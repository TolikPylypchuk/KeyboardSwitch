namespace KeyboardSwitch.Core.Services.Layout;

public sealed class NotSupportedLayoutLoaderService : ILayoutLoaderSrevice
{
    public bool IsLoadingLayoutsSupported => false;

    public IReadOnlyList<LoadableKeyboardLayout> GetAllSystemLayouts() =>
        throw new NotSupportedException("Getting all system layouts is not supported");

    public DisposableLayouts LoadLayouts(IEnumerable<LoadableKeyboardLayout> loadableLayouts) =>
        throw new NotSupportedException("Loading arbitrary layouts is not supported");
}

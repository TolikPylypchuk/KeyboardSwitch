namespace KeyboardSwitch.Core.Services.Layout;

public interface ILayoutLoaderSrevice
{
    bool IsLoadingLayoutsSupported { get; }

    IReadOnlyList<LoadableKeyboardLayout> GetAllSystemLayouts();
    DisposableLayouts LoadLayouts(IEnumerable<LoadableKeyboardLayout> loadableLayouts);
}

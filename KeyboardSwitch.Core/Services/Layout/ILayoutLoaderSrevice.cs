namespace KeyboardSwitch.Core.Services.Layout;

public interface ILayoutLoaderSrevice
{
    bool IsLoadingLayoutsSupported { get; }

    List<LoadableKeyboardLayout> GetAllSystemLayouts();
    DisposableLayouts LoadLayouts(IEnumerable<LoadableKeyboardLayout> loadableLayouts);
}

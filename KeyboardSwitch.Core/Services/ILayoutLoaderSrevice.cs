using System.Collections.Generic;

using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Core.Services
{
    public interface ILayoutLoaderSrevice
    {
        bool IsLoadingLayoutsSupported { get; }

        List<LoadableKeyboardLayout> GetAllSystemLayouts();
        DisposableLayouts LoadLayouts(IEnumerable<LoadableKeyboardLayout> loadableLayouts);
    }
}

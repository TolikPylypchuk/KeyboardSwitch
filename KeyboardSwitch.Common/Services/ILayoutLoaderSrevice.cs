using System.Collections.Generic;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
{
    public interface ILayoutLoaderSrevice
    {
        bool IsLoadingLayoutsSupported { get; }

        List<LoadableKeyboardLayout> GetAllSystemLayouts();
        DisposableLayouts LoadLayouts(IEnumerable<LoadableKeyboardLayout> loadableLayouts);
    }
}

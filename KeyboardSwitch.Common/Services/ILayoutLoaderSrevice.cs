using System.Collections.Generic;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
{
    public interface ILayoutLoaderSrevice
    {
        Dictionary<string, string> GetAllSystemLayouts();
        DisposableLayouts LoadLayouts(Dictionary<string, string> layouts);
    }
}

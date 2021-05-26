using System;
using System.Collections.Generic;

using KeyboardSwitch.Core.Keyboard;

namespace KeyboardSwitch.Core.Services.Layout
{
    public sealed class NotSupportedLayoutLoaderService : ILayoutLoaderSrevice
    {
        public bool IsLoadingLayoutsSupported => false;

        public List<LoadableKeyboardLayout> GetAllSystemLayouts() =>
            throw new NotSupportedException("Getting all system layouts is not supported");

        public DisposableLayouts LoadLayouts(IEnumerable<LoadableKeyboardLayout> loadableLayouts) =>
            throw new NotSupportedException("Loading arbitrary layouts is not supported");
    }
}

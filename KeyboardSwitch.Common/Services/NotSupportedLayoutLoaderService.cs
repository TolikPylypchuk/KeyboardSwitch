using System;
using System.Collections.Generic;

using KeyboardSwitch.Common.Keyboard;

namespace KeyboardSwitch.Common.Services
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
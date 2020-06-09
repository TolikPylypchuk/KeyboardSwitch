using System;
using System.Collections.Generic;

namespace KeyboardSwitch.Common.Keyboard
{
    public abstract class DisposableLayouts : IDisposable
    {
        protected DisposableLayouts(IEnumerable<KeyboardLayout> layouts)
            => this.Layouts = new List<KeyboardLayout>(layouts).AsReadOnly();

        ~DisposableLayouts()
            => this.Dispose(false);

        public IReadOnlyList<KeyboardLayout> Layouts { get; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}

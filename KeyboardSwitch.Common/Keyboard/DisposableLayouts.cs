using System;
using System.Collections.Generic;

namespace KeyboardSwitch.Common.Keyboard
{
    public sealed class DisposableLayouts : Disposable
    {
        private readonly IDisposable layoutDisposable;

        public DisposableLayouts(IEnumerable<KeyboardLayout> layouts, IDisposable layoutDisposable)
        {
            this.Layouts = new List<KeyboardLayout>(layouts).AsReadOnly();
            this.layoutDisposable = layoutDisposable;
        }

        public IReadOnlyList<KeyboardLayout> Layouts { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.layoutDisposable.Dispose();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using KeyboardSwitch.Common.Keyboard;

using static Vanara.PInvoke.User32;

namespace KeyboardSwitch.Windows
{
    internal sealed class UnloadableLayouts : DisposableLayouts
    {
        private readonly ISet<int> layoutIdsNotToUnload;

        public UnloadableLayouts(IEnumerable<KeyboardLayout> layouts, List<KeyboardLayout> layoutsNotToUnload)
            : base(layouts)
            => this.layoutIdsNotToUnload = layoutsNotToUnload.Select(layout => layout.Id).ToHashSet();

        protected override void Dispose(bool disposing)
            => this.Layouts
                .Select(layout => layout.Id)
                .Where(id => !this.layoutIdsNotToUnload.Contains(id))
                .ForEach(id => UnloadKeyboardLayout((IntPtr)id));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using KeyboardSwitch.Common.Keyboard;

using Microsoft.Extensions.Logging;

using static Vanara.PInvoke.User32;

namespace KeyboardSwitch.Windows
{
    internal sealed class UnloadableLayouts : DisposableLayouts
    {
        private readonly ISet<int> layoutIdsNotToUnload;
        private readonly ILogger? logger;

        public UnloadableLayouts(
            IEnumerable<KeyboardLayout> layouts,
            List<KeyboardLayout> layoutsNotToUnload,
            ILogger? logger = null)
            : base(layouts)
        {
            this.layoutIdsNotToUnload = layoutsNotToUnload.Select(layout => layout.Id).ToHashSet();
            this.logger = logger;
        }

        protected override void Dispose(bool disposing)
            => this.Layouts
                .Where(layout => !this.layoutIdsNotToUnload.Contains(layout.Id))
                .ForEach(layout =>
                {
                    UnloadKeyboardLayout((IntPtr)layout.Id);
                    this.logger?.LogInformation($"Unloaded layout: {layout.KeyboardName}");
                });
    }
}

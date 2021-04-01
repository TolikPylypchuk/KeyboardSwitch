using System.Collections.Generic;

namespace KeyboardSwitch.Settings.Core.Models
{
    public sealed class CharMappingModel
    {
        public List<LayoutModel> Layouts { get; set; } = new();
        public List<string> RemovableLayoutIds { get; set; } = new();
        public bool ShouldRemoveLayouts { get; set; } = false;
    }
}

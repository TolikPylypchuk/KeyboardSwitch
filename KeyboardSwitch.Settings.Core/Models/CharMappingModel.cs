using System.Collections.Generic;

namespace KeyboardSwitch.Settings.Core.Models
{
    public sealed class CharMappingModel
    {
        public List<LayoutModel> Layouts { get; set; } = new List<LayoutModel>();
        public List<int> RemovableLayoutIds { get; set; } = new List<int>();
        public bool ShouldRemoveLayouts { get; set; } = false;
    }
}

namespace KeyboardSwitch.Settings.Core.Models;

public sealed class CharMappingModel
{
    public List<LayoutModel> Layouts { get; set; } = [];
    public List<string> RemovableLayoutIds { get; set; } = [];
    public bool ShouldRemoveLayouts { get; set; } = false;
}

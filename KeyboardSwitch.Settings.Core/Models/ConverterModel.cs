using System.Collections.ObjectModel;

namespace KeyboardSwitch.Settings.Core.Models
{
    public sealed class ConverterModel
    {
        public ObservableCollection<CustomLayoutModel> Layouts { get; set; } = new();
    }
}

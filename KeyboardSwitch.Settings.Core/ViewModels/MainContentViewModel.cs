using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class MainContentViewModel : ReactiveObject
    {
        [Reactive]
        public CharMappingViewModel CharMappingViewModel { get; set; } = null!;

        [Reactive]
        public PreferencesViewModel PreferencesViewModel { get; set; } = null!;
    }
}

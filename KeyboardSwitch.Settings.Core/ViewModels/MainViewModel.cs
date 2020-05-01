using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly ISettingsService settingsService;
        private readonly ILayoutService layoutService;

        public MainViewModel(
            MainContentViewModel? mainContentViewModel = null,
            ServiceViewModel? serviceViewModel = null,
            ISettingsService? settingsService = null,
            ILayoutService? layoutService = null)
        {
            this.MainContentViewModel = mainContentViewModel ?? new MainContentViewModel();
            this.ServiceViewModel = serviceViewModel ?? new ServiceViewModel();

            this.settingsService = settingsService ?? Locator.Current.GetService<ISettingsService>();
            this.layoutService = layoutService ?? Locator.Current.GetService<ILayoutService>();

            this.OpenExternally = ReactiveCommand.Create(() => { });
        }

        [Reactive]
        public MainContentViewModel MainContentViewModel { get; set; }

        [Reactive]
        public ServiceViewModel ServiceViewModel { get; set; }

        public ReactiveCommand<Unit, Unit> OpenExternally { get; set; }

        public async Task LoadAsync()
        {
            var settings = await this.settingsService.GetSwitchSettingsAsync();
            var layouts = this.layoutService.GetKeyboardLayouts();

            var layoutsById = layouts.ToDictionary(layout => layout.Id, layout => layout);

            var layoutIndices = layouts.Select((layout, index) => (layout.Id, Index: index))
                .ToDictionary(item => item.Id, item => item.Index);

            var layoutModels =
                settings.CharsByKeyboardLayoutId
                    .Select(chars => new LayoutModel
                    {
                        Id = chars.Key,
                        Index = layoutIndices[chars.Key],
                        LanguageName = layoutsById[chars.Key].Culture.DisplayName,
                        KeyboardName = layoutsById[chars.Key].KeyboardName,
                        Chars = chars.Value
                            .Select((ch, index) => new CharacterModel { Character = ch, Index = index })
                            .ToList()
                    })
                    .ToList();

            this.MainContentViewModel.CharMappingViewModel = new CharMappingViewModel(
                new CharMappingModel { Layouts = layoutModels });
            this.MainContentViewModel.PreferencesViewModel = new PreferencesViewModel(new PreferencesModel());
        }
    }
}

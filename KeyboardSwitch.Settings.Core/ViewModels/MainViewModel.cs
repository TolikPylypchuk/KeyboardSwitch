using System.Linq;
using System.Reactive;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private readonly AppSettings appSettings;
        private readonly ILayoutService layoutService;

        public MainViewModel(
            AppSettings appSettings,
            ILayoutService? layoutService = null)
        {
            this.appSettings = appSettings;
            this.layoutService = layoutService ?? Locator.Current.GetService<ILayoutService>();

            this.MainContentViewModel = new MainContentViewModel(
                this.CreateCharMappingModel(), new PreferencesModel(appSettings));

            this.ServiceViewModel = new ServiceViewModel();

            this.OpenExternally = ReactiveCommand.Create(() => { });

            this.MainContentViewModel.Save
                .InvokeCommand(this.ServiceViewModel.ReloadSettings);
        }

        public MainContentViewModel MainContentViewModel { get; }
        public ServiceViewModel ServiceViewModel { get; }

        public ReactiveCommand<Unit, Unit> OpenExternally { get; }

        private CharMappingModel CreateCharMappingModel()
        {
            var layouts = this.layoutService.GetKeyboardLayouts();

            var layoutsById = layouts.ToDictionary(layout => layout.Id, layout => layout);

            var layoutIndices = layouts.Select((layout, index) => (layout.Id, Index: index))
                .ToDictionary(item => item.Id, item => item.Index);

            var layoutModels = this.appSettings.CharsByKeyboardLayoutId
                    .Select(chars => new LayoutModel
                    {
                        Id = chars.Key,
                        Index = layoutIndices[chars.Key],
                        LanguageName = layoutsById[chars.Key].Culture.DisplayName,
                        KeyboardName = layoutsById[chars.Key].KeyboardName,
                        Chars = chars.Value
                    })
                    .ToList();

            return new CharMappingModel { Layouts = layoutModels };
        }
    }
}

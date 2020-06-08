using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class CharMappingViewModel : ReactiveForm<CharMappingModel, CharMappingViewModel>
    {
        private readonly ILayoutService layoutService;
        private readonly IAutoConfigurationService autoConfigurationService;

        private readonly SourceCache<LayoutModel, int> layoutsSource =
            new SourceCache<LayoutModel, int>(layout => layout.Id);

        private readonly ReadOnlyObservableCollection<LayoutViewModel> layouts;

        public CharMappingViewModel(
            CharMappingModel charMappingModel,
            ILayoutService? layoutService = null,
            IAutoConfigurationService? autoConfigurationService = null,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.CharMappingModel = charMappingModel;

            this.layoutService = layoutService ?? Locator.Current.GetService<ILayoutService>();
            this.autoConfigurationService = autoConfigurationService ??
                Locator.Current.GetService<IAutoConfigurationService>();

            this.layoutsSource.Connect()
                .Transform(ch => new LayoutViewModel(ch))
                .Sort(SortExpressionComparer<LayoutViewModel>.Ascending(vm => vm.Index))
                .Bind(out this.layouts)
                .Subscribe();

            var canAutoConfigure = this.Layouts
                .ToObservableChangeSet()
                .AutoRefreshOnObservable(layout => layout.Changed)
                .ToCollection()
                .Select(layouts => layouts.All(layout => String.IsNullOrEmpty(layout.Chars)));

            this.AutoConfigure = ReactiveCommand.Create(this.OnAutoConfigure, canAutoConfigure);

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public CharMappingModel CharMappingModel { get; }

        public ReadOnlyObservableCollection<LayoutViewModel> Layouts
            => this.layouts;

        public ReactiveCommand<Unit, Unit> AutoConfigure { get; }

        protected override CharMappingViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(this.IsCollectionChanged(vm => vm.Layouts, vm => vm.CharMappingModel.Layouts));

            base.EnableChangeTracking();
        }

        protected override async Task<CharMappingModel> OnSaveAsync()
        {
            foreach (var layout in this.Layouts)
            {
                await layout.Save.Execute();
            }

            this.CharMappingModel.Layouts.Clear();
            this.CharMappingModel.Layouts.AddRange(this.layoutsSource.Items);

            return this.CharMappingModel;
        }

        protected override void CopyProperties()
            => this.layoutsSource.Edit(list =>
            {
                list.Clear();
                list.AddOrUpdate(this.CharMappingModel.Layouts);
            });

        private void OnAutoConfigure()
        {
            var layouts = this.layoutService.GetKeyboardLayouts();
            var charsByLayoutId = this.autoConfigurationService.CreateCharMappings(layouts);

            foreach (var layoutAndChars in charsByLayoutId)
            {
                var layoutViewModel = this.Layouts.First(layout => layout.Id == layoutAndChars.Key);
                layoutViewModel.Chars = layoutAndChars.Value;
            }
        }
    }
}

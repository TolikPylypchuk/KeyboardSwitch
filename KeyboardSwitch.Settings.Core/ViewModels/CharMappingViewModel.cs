using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Settings.Core.Models;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class CharMappingViewModel : FormBase<CharMappingModel, CharMappingViewModel>
    {
        private readonly SourceCache<LayoutModel, int> layoutsSource =
            new SourceCache<LayoutModel, int>(layout => layout.Id);

        private readonly ReadOnlyObservableCollection<LayoutViewModel> layouts;

        public CharMappingViewModel(CharMappingModel charMappingModel)
        {
            this.CharMappingModel = charMappingModel;

            this.layoutsSource.Connect()
                .Transform(ch => new LayoutViewModel(ch))
                .Sort(SortExpressionComparer<LayoutViewModel>.Ascending(vm => vm.Index))
                .Bind(out this.layouts)
                .Subscribe();

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public CharMappingModel CharMappingModel { get; }

        public ReadOnlyObservableCollection<LayoutViewModel> Layouts
            => this.layouts;

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
        {
            layoutsSource.Edit(list =>
            {
                list.Clear();
                list.AddOrUpdate(this.CharMappingModel.Layouts);
            });
        }
    }
}

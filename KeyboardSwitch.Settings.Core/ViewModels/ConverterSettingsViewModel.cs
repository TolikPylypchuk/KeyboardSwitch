using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using DynamicData;

using KeyboardSwitch.Settings.Core.Models;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class ConverterSettingsViewModel : FormBase<ConverterModel, ConverterSettingsViewModel>
    {
        private readonly SourceList<CustomLayoutModel> customLayoutsSource = new SourceList<CustomLayoutModel>();
        private readonly ReadOnlyObservableCollection<CustomLayoutViewModel> customLayouts;

        public ConverterSettingsViewModel(
            ConverterModel converterModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.ConverterModel = converterModel;

            this.customLayoutsSource.Connect()
                .Transform(model => new CustomLayoutViewModel(model, this.ResourceManager, this.Scheduler))
                .Bind(out this.customLayouts)
                .Subscribe();

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public ConverterModel ConverterModel { get; }

        public ReadOnlyObservableCollection<CustomLayoutViewModel> CustomLayouts
            => this.customLayouts;

        protected override ConverterSettingsViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(this.IsCollectionChanged(vm => vm.CustomLayouts, vm => vm.ConverterModel.Layouts));
            base.EnableChangeTracking();
        }

        protected override async Task<ConverterModel> OnSaveAsync()
        {
            foreach (var layout in this.CustomLayouts)
            {
                await layout.Save.Execute();
            }

            this.ConverterModel.Layouts.Clear();
            this.ConverterModel.Layouts.AddRange(this.CustomLayouts.Select(vm => vm.CustomLayoutModel));

            return this.ConverterModel;
        }

        protected override void CopyProperties()
        {
            this.customLayoutsSource.Edit(list =>
            {
                list.Clear();
                list.AddRange(this.ConverterModel.Layouts);
            });
        }
    }
}

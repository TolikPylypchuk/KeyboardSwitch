using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using DynamicData;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class ConverterSettingsViewModel : ReactiveForm<ConverterModel, ConverterSettingsViewModel>
    {
        private readonly SourceList<CustomLayoutModel> customLayoutsSource = new SourceList<CustomLayoutModel>();
        private readonly ReadOnlyObservableCollection<CustomLayoutViewModel> customLayouts;
        private readonly Dictionary<CustomLayoutViewModel, IDisposable> customLayoutSubscriptions =
            new Dictionary<CustomLayoutViewModel, IDisposable>();

        public ConverterSettingsViewModel(
            ConverterModel converterModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.ConverterModel = converterModel;

            this.customLayoutsSource.Connect()
                .Transform(model => this.CreateCustomLayoutViewModel(model, isNew: false))
                .Bind(out this.customLayouts)
                .Subscribe();

            this.AddCustomLayout = ReactiveCommand.Create(this.OnAddCustomLayout);

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public ConverterModel ConverterModel { get; }

        public ReadOnlyObservableCollection<CustomLayoutViewModel> CustomLayouts
            => this.customLayouts;

        public ReactiveCommand<Unit, Unit> AddCustomLayout { get; set; }

        protected override ConverterSettingsViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(this.IsCollectionChanged(vm => vm.CustomLayouts, vm => vm.ConverterModel.Layouts));
            this.TrackValidation(this.IsCollectionValid(this.CustomLayouts));

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
            foreach (var subscription in this.customLayoutSubscriptions)
            {
                subscription.Value.Dispose();
            }

            this.customLayoutSubscriptions.Clear();

            this.customLayoutsSource.Edit(list =>
            {
                list.Clear();
                list.AddRange(this.ConverterModel.Layouts);
            });
        }

        private CustomLayoutViewModel CreateCustomLayoutViewModel(CustomLayoutModel model, bool isNew)
        {
            var viewModel = new CustomLayoutViewModel(model, isNew, this.ResourceManager, this.Scheduler);

            var deleteSubscription = viewModel.Delete
                .WhereNotNull()
                .Subscribe(deletedLayout =>
                {
                    this.customLayoutsSource.Remove(deletedLayout);
                    this.customLayoutSubscriptions[viewModel].Dispose();
                    this.customLayoutSubscriptions.Remove(viewModel);
                });

            this.customLayoutSubscriptions.Add(viewModel, deleteSubscription);

            return viewModel;
        }

        private void OnAddCustomLayout()
            => this.customLayoutsSource.Add(new CustomLayoutModel
            {
                Id = this.customLayoutsSource.Items.Max(model => (int?)model.Id) ?? 1
            });
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Resources;
using System.Threading.Tasks;

using DynamicData;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class ConverterSettingsViewModel : ReactiveForm<ConverterModel, ConverterSettingsViewModel>
    {
        private readonly ILayoutLoaderSrevice layoutLoaderService;
        private readonly IAutoConfigurationService autoConfigurationService;

        private readonly SourceList<CustomLayoutModel> customLayoutsSource = new SourceList<CustomLayoutModel>();
        private readonly ReadOnlyObservableCollection<CustomLayoutViewModel> customLayouts;
        private readonly Dictionary<CustomLayoutViewModel, IDisposable> customLayoutSubscriptions =
            new Dictionary<CustomLayoutViewModel, IDisposable>();

        private readonly BehaviorSubject<bool> isAutoConfiguringLayoutsSubject = new BehaviorSubject<bool>(false);
        private readonly CompositeDisposable loadableLayoutsSettingsSubscriptions = new CompositeDisposable();

        public ConverterSettingsViewModel(
            ConverterModel converterModel,
            ILayoutLoaderSrevice? layoutLoaderService = null,
            IAutoConfigurationService? autoConfigurationService = null,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.ConverterModel = converterModel;

            this.layoutLoaderService = layoutLoaderService ?? Locator.Current.GetService<ILayoutLoaderSrevice>();
            this.autoConfigurationService = autoConfigurationService
                ?? Locator.Current.GetService<IAutoConfigurationService>();

            this.customLayoutsSource.Connect()
                .Transform(model => this.CreateCustomLayoutViewModel(model, isNew: false))
                .Bind(out this.customLayouts)
                .Subscribe();

            this.isAutoConfiguringLayoutsSubject
                .ToPropertyEx(this, vm => vm.IsAutoConfiguringLayouts);

            this.AddCustomLayout = ReactiveCommand.Create(this.OnAddCustomLayout);
            this.AutoConfigureCustomLayouts = ReactiveCommand.Create(this.OnAutoConfigureCustomLayouts);

            this.AutoConfigureCustomLayouts
                .Select(_ => true)
                .Subscribe(this.isAutoConfiguringLayoutsSubject);

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public ConverterModel ConverterModel { get; }

        public bool IsAutoConfiguringLayouts { [ObservableAsProperty] get; }

        public ReadOnlyObservableCollection<CustomLayoutViewModel> CustomLayouts
            => this.customLayouts;

        [Reactive]
        public LoadableLayoutsSettingsViewModel? LoadableLayoutsSettingsViewModel { get; private set; }

        public ReactiveCommand<Unit, Unit> AddCustomLayout { get; set; }
        public ReactiveCommand<Unit, Unit> AutoConfigureCustomLayouts { get; set; }

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

        private void OnAutoConfigureCustomLayouts()
        {
            this.LoadableLayoutsSettingsViewModel = new LoadableLayoutsSettingsViewModel(
                this.layoutLoaderService.GetAllSystemLayouts());

            this.LoadableLayoutsSettingsViewModel.Finish
                .Subscribe(this.OnAutoConfigureCustomLayoutsFinish)
                .DisposeWith(this.loadableLayoutsSettingsSubscriptions);

            this.LoadableLayoutsSettingsViewModel.Finish
                .Select(_ => false)
                .Subscribe(this.isAutoConfiguringLayoutsSubject)
                .DisposeWith(this.loadableLayoutsSettingsSubscriptions);
        }

        private void OnAutoConfigureCustomLayoutsFinish(bool shouldLoad)
        {
            if (shouldLoad && this.LoadableLayoutsSettingsViewModel != null)
            {
                using var disposableLayouts = this.layoutLoaderService.LoadLayouts(
                    this.LoadableLayoutsSettingsViewModel.AddedLayouts);

                var configuredMappings = this.autoConfigurationService.CreateCharMappings(disposableLayouts.Layouts);

                foreach (var mapping in configuredMappings)
                {
                    var name = disposableLayouts.Layouts.First(layout => layout.Id == mapping.Key).KeyboardName;

                    this.customLayoutsSource.Add(new CustomLayoutModel
                    {
                        Id = this.customLayoutsSource.Count + 1,
                        Name = name,
                        Chars = mapping.Value
                    });
                }
            }

            this.LoadableLayoutsSettingsViewModel = null;
        }
    }
}

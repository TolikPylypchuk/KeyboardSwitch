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
using DynamicData.Binding;

using KeyboardSwitch.Core;
using KeyboardSwitch.Core.Services.AutoConfiguration;
using KeyboardSwitch.Core.Services.Layout;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;

using static KeyboardSwitch.Settings.Core.ServiceUtil;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class ConverterSettingsViewModel : ReactiveForm<ConverterModel, ConverterSettingsViewModel>
    {
        private readonly ILayoutLoaderSrevice layoutLoaderService;
        private readonly IAutoConfigurationService autoConfigurationService;

        private readonly SourceList<CustomLayoutModel> customLayoutsSource = new();
        private readonly ReadOnlyObservableCollection<CustomLayoutViewModel> customLayouts;
        private readonly Dictionary<CustomLayoutViewModel, IDisposable> customLayoutSubscriptions = new();

        private readonly BehaviorSubject<bool> isAutoConfiguringLayoutsSubject = new(false);
        private readonly CompositeDisposable loadableLayoutsSettingsSubscriptions = new();

        public ConverterSettingsViewModel(
            ConverterModel converterModel,
            ILayoutLoaderSrevice? layoutLoaderService = null,
            IAutoConfigurationService? autoConfigurationService = null,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.ConverterModel = converterModel;

            this.layoutLoaderService = layoutLoaderService ?? GetDefaultService<ILayoutLoaderSrevice>();
            this.autoConfigurationService = autoConfigurationService ?? GetDefaultService<IAutoConfigurationService>();

            this.customLayoutsSource.Connect()
                .Transform(model => this.CreateCustomLayoutViewModel(model, isNew: false))
                .Bind(out this.customLayouts)
                .Subscribe();

            this.isAutoConfiguringLayoutsSubject
                .ToPropertyEx(this, vm => vm.IsAutoConfiguringLayouts);

            var namesAreUnique = this.customLayouts.ToObservableChangeSet()
                .AutoRefresh()
                .ToCollection()
                .Select(layouts => layouts.Select(layout => layout.Name).Distinct().Count() == layouts.Count)
                .StartWith(true);

            this.LayoutNamesAreUniqueRule = this.LocalizedValidationRule(namesAreUnique, "CustomLayoutNamesAreSame");

            this.AddCustomLayout = ReactiveCommand.Create(this.OnAddCustomLayout);
            this.AutoConfigureCustomLayouts = ReactiveCommand.Create(
                this.OnAutoConfigureCustomLayouts,
                Observable.Return(this.layoutLoaderService.IsLoadingLayoutsSupported));

            this.AutoConfigureCustomLayouts
                .Select(_ => true)
                .Subscribe(this.isAutoConfiguringLayoutsSubject);

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public ConverterModel ConverterModel { get; }

        public bool IsAutoConfiguringLayouts { [ObservableAsProperty] get; }

        public ReadOnlyObservableCollection<CustomLayoutViewModel> CustomLayouts => this.customLayouts;

        [Reactive]
        public LoadableLayoutsSettingsViewModel? LoadableLayoutsSettingsViewModel { get; private set; }

        public ValidationHelper LayoutNamesAreUniqueRule { get; }

        public ReactiveCommand<Unit, Unit> AddCustomLayout { get; set; }
        public ReactiveCommand<Unit, Unit> AutoConfigureCustomLayouts { get; set; }

        protected override ConverterSettingsViewModel Self => this;

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

        private void OnAddCustomLayout() =>
            this.customLayoutsSource.Add(new CustomLayoutModel
            {
                Id = this.customLayoutsSource.Items.Max(model => (int?)model.Id) ?? 1
            });

        private void OnAutoConfigureCustomLayouts()
        {
            this.LoadableLayoutsSettingsViewModel = new(this.layoutLoaderService.GetAllSystemLayouts());

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
                    this.LoadableLayoutsSettingsViewModel.AddedLayouts.Select(vm => vm.Layout));

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

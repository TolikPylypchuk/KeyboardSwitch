using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Common.Keyboard;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class LoadableLayoutsSettingsViewModel : ReactiveObject
    {
        private readonly SourceCache<LoadableKeyboardLayout, string> addableLayoutsSource =
            new SourceCache<LoadableKeyboardLayout, string>(layout => layout.Tag);

        private readonly SourceCache<LoadableKeyboardLayout, string> addedLayoutsSource =
            new SourceCache<LoadableKeyboardLayout, string>(layout => layout.Tag);

        private readonly ReadOnlyObservableCollection<LoadableLayoutViewModel> addableLayouts;
        private readonly ReadOnlyObservableCollection<LoadableLayoutViewModel> addedLayouts;

        public LoadableLayoutsSettingsViewModel(IEnumerable<LoadableKeyboardLayout> layouts)
        {
            var comparer = SortExpressionComparer<LoadableLayoutViewModel>.Ascending(layout => layout.Layout.Name);

            this.addableLayoutsSource.Connect()
                .Transform(this.CreateLayoutViewModel)
                .Sort(comparer)
                .Bind(out this.addableLayouts)
                .Subscribe();

            this.addedLayoutsSource.Connect()
                .Transform(this.CreateLayoutViewModel)
                .Bind(out this.addedLayouts)
                .Subscribe();

            this.AddLayout = ReactiveCommand.Create<LoadableLayoutViewModel>(this.OnAddLayout);
            this.RemoveLayout = ReactiveCommand.Create<LoadableLayoutViewModel>(this.OnRemoveLayout);
            this.Finish = ReactiveCommand.Create<bool, bool>(shouldLoad => shouldLoad);

            this.addableLayoutsSource.AddOrUpdate(layouts);
        }

        public ReadOnlyObservableCollection<LoadableLayoutViewModel> AddableLayouts
            => this.addableLayouts;

        public ReadOnlyObservableCollection<LoadableLayoutViewModel> AddedLayouts
            => this.addedLayouts;

        public ReactiveCommand<LoadableLayoutViewModel, Unit> AddLayout { get; }
        public ReactiveCommand<LoadableLayoutViewModel, Unit> RemoveLayout { get; }
        public ReactiveCommand<bool, bool> Finish { get; }

        private LoadableLayoutViewModel CreateLayoutViewModel(LoadableKeyboardLayout layout)
        {
            var vm = new LoadableLayoutViewModel(layout);
            var subscriptions = new CompositeDisposable();

            vm.Delete
                .Select(_ => vm)
                .InvokeCommand(this.RemoveLayout)
                .DisposeWith(subscriptions);

            vm.Delete
                .Subscribe(_ => subscriptions.Dispose())
                .DisposeWith(subscriptions);

            return vm;
        }

        private void OnAddLayout(LoadableLayoutViewModel vm)
        {
            this.addableLayoutsSource.Remove(vm.Layout.Tag);
            this.addedLayoutsSource.AddOrUpdate(vm.Layout);
        }

        private void OnRemoveLayout(LoadableLayoutViewModel vm)
        {
            this.addedLayoutsSource.Remove(vm.Layout.Tag);
            this.addableLayoutsSource.AddOrUpdate(vm.Layout);
        }
    }
}

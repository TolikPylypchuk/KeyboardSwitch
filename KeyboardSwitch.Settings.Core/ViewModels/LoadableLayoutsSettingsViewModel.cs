using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Common.Keyboard;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class LoadableLayoutsSettingsViewModel
    {
        private readonly SourceCache<LoadableKeyboardLayout, string> addableLayoutsSource =
            new SourceCache<LoadableKeyboardLayout, string>(layout => layout.Tag);

        private readonly SourceCache<LoadableKeyboardLayout, string> addedLayoutsSource =
            new SourceCache<LoadableKeyboardLayout, string>(layout => layout.Tag);

        private readonly ReadOnlyObservableCollection<LoadableKeyboardLayout> addableLayouts;
        private readonly ReadOnlyObservableCollection<LoadableKeyboardLayout> addedLayouts;

        public LoadableLayoutsSettingsViewModel(IEnumerable<LoadableKeyboardLayout> layouts)
        {
            var comparer = SortExpressionComparer<LoadableKeyboardLayout>.Ascending(layout => layout.Name);

            this.addableLayoutsSource.Connect()
                .Sort(comparer)
                .Bind(out this.addableLayouts)
                .Subscribe();

            this.addedLayoutsSource.Connect()
                .Bind(out this.addedLayouts)
                .Subscribe();

            this.addableLayoutsSource.AddOrUpdate(layouts);

            this.AddLayout = ReactiveCommand.Create<LoadableKeyboardLayout>(this.OnAddLayout);
            this.RemoveLayout = ReactiveCommand.Create<LoadableKeyboardLayout>(this.OnRemoveLayout);
            this.Finish = ReactiveCommand.Create<bool, bool>(shouldLoad => shouldLoad);
        }

        public ReadOnlyObservableCollection<LoadableKeyboardLayout> AddableLayouts
            => this.addableLayouts;

        public ReadOnlyObservableCollection<LoadableKeyboardLayout> AddedLayouts
            => this.addedLayouts;

        public ReactiveCommand<LoadableKeyboardLayout, Unit> AddLayout { get; }
        public ReactiveCommand<LoadableKeyboardLayout, Unit> RemoveLayout { get; }
        public ReactiveCommand<bool, bool> Finish { get; }

        private void OnAddLayout(LoadableKeyboardLayout layout)
        {
            this.addableLayoutsSource.Remove(layout.Tag);
            this.addedLayoutsSource.AddOrUpdate(layout);
        }

        private void OnRemoveLayout(LoadableKeyboardLayout layout)
        {
            this.addedLayoutsSource.Remove(layout.Tag);
            this.addableLayoutsSource.AddOrUpdate(layout);
        }
    }
}

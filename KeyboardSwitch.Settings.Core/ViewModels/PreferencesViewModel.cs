using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class PreferencesViewModel : ReactiveForm<PreferencesModel, PreferencesViewModel>
    {
        private readonly SourceList<ModifierKey> forwardModifierKeysSource = new();
        private readonly SourceList<ModifierKey> backwardModifierKeysSource = new();

        private readonly ReadOnlyObservableCollection<ModifierKey> forwardModifierKeys;
        private readonly ReadOnlyObservableCollection<ModifierKey> backwardModifierKeys;

        public PreferencesViewModel(
            PreferencesModel preferencesModel,
            bool canShowConverter,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.PreferencesModel = preferencesModel;
            this.CanShowConverter = canShowConverter;
            this.CopyProperties();

            this.forwardModifierKeysSource.Connect()
                .Bind(out this.forwardModifierKeys)
                .Subscribe();

            this.backwardModifierKeysSource.Connect()
                .Bind(out this.backwardModifierKeys)
                .Subscribe();

            this.BindKeys();

            this.ValidationRule(vm => vm.PressCount, count => count > 0 && count <= 10);
            this.ValidationRule(vm => vm.WaitMilliseconds, wait => wait >= 100 && wait <= 1000);

            this.ModifierKeysAreDifferentRule = this.InitModifierKeysAreDifferentRule();
            this.SwitchMethodsAreDifferentRule = this.InitSwitchMethodsAreDifferentRule();

            this.EnableChangeTracking();
        }

        public PreferencesModel PreferencesModel { get; }

        [Reactive]
        public bool InstantSwitching { get; set; }

        [Reactive]
        public bool SwitchLayout { get; set; }

        [Reactive]
        public bool Startup { get; set; }

        [Reactive]
        public bool ShowUninstalledLayoutsMessage { get; set; }

        [Reactive]
        public bool ShowConverter { get; set; }

        [Reactive]
        public ModifierKey ForwardModifierKeyFirst { get; set; }

        [Reactive]
        public ModifierKey ForwardModifierKeySecond { get; set; }

        [Reactive]
        public ModifierKey ForwardModifierKeyThird { get; set; }

        [Reactive]
        public ModifierKey BackwardModifierKeyFirst { get; set; }

        [Reactive]
        public ModifierKey BackwardModifierKeySecond { get; set; }

        [Reactive]
        public ModifierKey BackwardModifierKeyThird { get; set; }

        [Reactive]
        public int PressCount { get; set; }

        [Reactive]
        public int WaitMilliseconds { get; set; }

        public bool CanShowConverter { get; }

        public ValidationHelper ModifierKeysAreDifferentRule { get; }
        public ValidationHelper SwitchMethodsAreDifferentRule { get; }

        protected override PreferencesViewModel Self => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.InstantSwitching, vm => vm.PreferencesModel.InstantSwitching);
            this.TrackChanges(vm => vm.SwitchLayout, vm => vm.PreferencesModel.SwitchLayout);
            this.TrackChanges(vm => vm.Startup, vm => vm.PreferencesModel.Startup);

            this.TrackChanges(
                vm => vm.ShowUninstalledLayoutsMessage, vm => vm.PreferencesModel.ShowUninstalledLayoutsMessage);

            this.TrackChanges(vm => vm.ShowConverter, vm => vm.PreferencesModel.ShowConverter);

            this.TrackChanges(this.IsCollectionChangedSimple(
                vm => vm.forwardModifierKeys, vm => vm.PreferencesModel.SwitchSettings.ForwardModifierKeys));

            this.TrackChanges(this.IsCollectionChangedSimple(
                vm => vm.backwardModifierKeys, vm => vm.PreferencesModel.SwitchSettings.BackwardModifierKeys));

            this.TrackChanges(vm => vm.PressCount, vm => vm.PreferencesModel.SwitchSettings.PressCount);
            this.TrackChanges(vm => vm.WaitMilliseconds, vm => vm.PreferencesModel.SwitchSettings.WaitMilliseconds);

            base.EnableChangeTracking();
        }

        protected override Task<PreferencesModel> OnSaveAsync()
        {
            this.PreferencesModel.InstantSwitching = this.InstantSwitching;
            this.PreferencesModel.SwitchLayout = this.SwitchLayout;
            this.PreferencesModel.Startup = this.Startup;
            this.PreferencesModel.ShowUninstalledLayoutsMessage = this.ShowUninstalledLayoutsMessage;
            this.PreferencesModel.ShowConverter = this.ShowConverter;

            this.PreferencesModel.SwitchSettings.ForwardModifierKeys = new(this.forwardModifierKeys);
            this.PreferencesModel.SwitchSettings.BackwardModifierKeys = new(this.backwardModifierKeys);
            this.PreferencesModel.SwitchSettings.PressCount = this.PressCount;
            this.PreferencesModel.SwitchSettings.WaitMilliseconds = this.WaitMilliseconds;

            return Task.FromResult(this.PreferencesModel);
        }

        protected override void CopyProperties()
        {
            this.InstantSwitching = this.PreferencesModel.InstantSwitching;
            this.SwitchLayout = this.PreferencesModel.SwitchLayout;
            this.Startup = this.PreferencesModel.Startup;
            this.ShowUninstalledLayoutsMessage = this.PreferencesModel.ShowUninstalledLayoutsMessage;
            this.ShowConverter = this.PreferencesModel.ShowConverter;

            if (this.PreferencesModel.SwitchSettings.ForwardModifierKeys.Count > 0)
            {
                this.ForwardModifierKeyFirst = this.PreferencesModel.SwitchSettings.ForwardModifierKeys[0];
            }

            if (this.PreferencesModel.SwitchSettings.ForwardModifierKeys.Count > 1)
            {
                this.ForwardModifierKeySecond = this.PreferencesModel.SwitchSettings.ForwardModifierKeys[1];
            }

            if (this.PreferencesModel.SwitchSettings.ForwardModifierKeys.Count > 2)
            {
                this.ForwardModifierKeyThird = this.PreferencesModel.SwitchSettings.ForwardModifierKeys[2];
            }

            if (this.PreferencesModel.SwitchSettings.BackwardModifierKeys.Count > 0)
            {
                this.BackwardModifierKeyFirst = this.PreferencesModel.SwitchSettings.BackwardModifierKeys[0];
            }

            if (this.PreferencesModel.SwitchSettings.BackwardModifierKeys.Count > 1)
            {
                this.BackwardModifierKeySecond = this.PreferencesModel.SwitchSettings.BackwardModifierKeys[1];
            }

            if (this.PreferencesModel.SwitchSettings.BackwardModifierKeys.Count > 2)
            {
                this.BackwardModifierKeyThird = this.PreferencesModel.SwitchSettings.BackwardModifierKeys[2];
            }

            this.PressCount = this.PreferencesModel.SwitchSettings.PressCount;
            this.WaitMilliseconds = this.PreferencesModel.SwitchSettings.WaitMilliseconds;
        }

        private void BindKeys()
        {
            this.WhenAnyValue(
                vm => vm.ForwardModifierKeyFirst,
                vm => vm.ForwardModifierKeySecond,
                vm => vm.ForwardModifierKeyThird)
                .Subscribe(keys =>
                    this.forwardModifierKeysSource.Edit(list =>
                    {
                        list.Clear();
                        list.Add(keys.Item1);
                        list.Add(keys.Item2);
                        list.Add(keys.Item3);
                    }));

            this.WhenAnyValue(
                vm => vm.BackwardModifierKeyFirst,
                vm => vm.BackwardModifierKeySecond,
                vm => vm.BackwardModifierKeyThird)
                .Subscribe(keys =>
                    this.backwardModifierKeysSource.Edit(list =>
                    {
                        list.Clear();
                        list.Add(keys.Item1);
                        list.Add(keys.Item2);
                        list.Add(keys.Item3);
                    }));
        }

        private ValidationHelper InitModifierKeysAreDifferentRule()
        {
            var modifierKeysAreDifferent = Observable.CombineLatest(
                this.forwardModifierKeys
                    .ToObservableChangeSet()
                    .ToCollection()
                    .Select(this.ContainsDistinctElements),
                this.backwardModifierKeys
                    .ToObservableChangeSet()
                    .ToCollection()
                    .Select(this.ContainsDistinctElements),
                (forward, backward) => forward && backward);

            return this.LocalizedValidationRule(modifierKeysAreDifferent, "ModifierKeysAreSame");
        }

        private ValidationHelper InitSwitchMethodsAreDifferentRule()
        {
            var switchMethodsAreDifferent = Observable.CombineLatest(
                this.forwardModifierKeys.ToObservableChangeSet().ToCollection(),
                this.backwardModifierKeys.ToObservableChangeSet().ToCollection(),
                (forward, backward) => !new HashSet<ModifierKey>(forward).SetEquals(backward));

            return this.LocalizedValidationRule(switchMethodsAreDifferent, "SwitchMethodsAreSame");
        }

        private bool ContainsDistinctElements(IReadOnlyCollection<ModifierKey> keys) =>
            !keys
                .SelectMany((x, index1) =>
                    keys.Select((y, index2) => (Key1: x, Key2: y, Index1: index1, Index2: index2)))
                .Where(keys => keys.Index1 < keys.Index2)
                .Select(keys => (keys.Key1 & keys.Key2) == ModifierKey.None)
                .Any(equals => !equals);
    }
}

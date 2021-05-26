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

using KeyboardSwitch.Core;
using KeyboardSwitch.Core.Keyboard;
using KeyboardSwitch.Core.Services.Layout;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class PreferencesViewModel : ReactiveForm<PreferencesModel, PreferencesViewModel>
    {
        private readonly SourceList<ModifierKey> forwardModifierKeysSource = new();
        private readonly SourceList<ModifierKey> backwardModifierKeysSource = new();

        private readonly SourceCache<KeyCode, KeyCode> layoutForwardKeyCodesSource = new(keyCode => keyCode);
        private readonly SourceCache<KeyCode, KeyCode> layoutBackwardKeyCodesSource = new(keyCode => keyCode);

        private readonly ReadOnlyObservableCollection<ModifierKey> forwardModifierKeys;
        private readonly ReadOnlyObservableCollection<ModifierKey> backwardModifierKeys;

        private readonly ReadOnlyObservableCollection<KeyCode> layoutForwardKeyCodes;
        private readonly ReadOnlyObservableCollection<KeyCode> layoutBackwardKeyCodes;

        public PreferencesViewModel(
            PreferencesModel preferencesModel,
            ILayoutService? layoutService = null,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.PreferencesModel = preferencesModel;
            this.CopyProperties();

            this.forwardModifierKeysSource.Connect()
                .Bind(out this.forwardModifierKeys)
                .Subscribe();

            this.backwardModifierKeysSource.Connect()
                .Bind(out this.backwardModifierKeys)
                .Subscribe();

            this.layoutForwardKeyCodesSource.Connect()
                .Bind(out this.layoutForwardKeyCodes)
                .Subscribe();

            this.layoutBackwardKeyCodesSource.Connect()
                .Bind(out this.layoutBackwardKeyCodes)
                .Subscribe();

            this.BindKeys();

            this.AddLayoutForwardKeyCode = ReactiveCommand.Create<KeyCode>(
                this.layoutForwardKeyCodesSource.AddOrUpdate);

            this.AddLayoutBackwardKeyCode = ReactiveCommand.Create<KeyCode>(
                this.layoutBackwardKeyCodesSource.AddOrUpdate);

            this.ClearLayoutForwardKeyCodes = ReactiveCommand.Create(this.layoutForwardKeyCodesSource.Clear);
            this.ClearLayoutBackwardKeyCodes = ReactiveCommand.Create(this.layoutBackwardKeyCodesSource.Clear);

            this.LocalizedValidationRule(vm => vm.PressCount, count => count > 0 && count <= 10);
            this.LocalizedValidationRule(vm => vm.WaitMilliseconds, wait => wait >= 100 && wait <= 1000);

            this.ModifierKeysAreDifferentRule = this.InitModifierKeysAreDifferentRule();
            this.SwitchMethodsAreDifferentRule = this.InitSwitchMethodsAreDifferentRule();

            this.LayoutForwardKeysAreNotEmptyRule = this.InitLayoutKeysAreNotEmptyRule(
                this.layoutForwardKeyCodesSource, "LayoutForwardKeysEmpty");

            this.LayoutBackwardKeysAreNotEmptyRule = this.InitLayoutKeysAreNotEmptyRule(
                this.layoutBackwardKeyCodesSource, "LayoutBackwardKeysEmpty");

            this.LayoutKeysAreDifferentRule = this.InitLayoutKeysAreDifferentRule();

            layoutService ??= Locator.Current.GetService<ILayoutService>();

            this.SwitchLayoutsViaKeyboardSimulation = layoutService?.SwitchLayoutsViaKeyboardSimulation ?? false;

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

        public ReadOnlyObservableCollection<KeyCode> LayoutForwardKeyCodes =>
            this.layoutForwardKeyCodes;

        public ReadOnlyObservableCollection<KeyCode> LayoutBackwardKeyCodes =>
            this.layoutBackwardKeyCodes;

        public ReactiveCommand<KeyCode, Unit> AddLayoutForwardKeyCode { get; }
        public ReactiveCommand<KeyCode, Unit> AddLayoutBackwardKeyCode { get; }

        public ReactiveCommand<Unit, Unit> ClearLayoutForwardKeyCodes { get; }
        public ReactiveCommand<Unit, Unit> ClearLayoutBackwardKeyCodes { get; }

        public ValidationHelper ModifierKeysAreDifferentRule { get; }
        public ValidationHelper SwitchMethodsAreDifferentRule { get; }

        public ValidationHelper LayoutForwardKeysAreNotEmptyRule { get; }
        public ValidationHelper LayoutBackwardKeysAreNotEmptyRule { get; }
        public ValidationHelper LayoutKeysAreDifferentRule { get; }

        public bool SwitchLayoutsViaKeyboardSimulation { get; }

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

            this.TrackChanges(this.IsCollectionChangedSimple(
                vm => vm.LayoutForwardKeyCodes, vm => vm.PreferencesModel.SwitchSettings.LayoutForwardKeys));

            this.TrackChanges(this.IsCollectionChangedSimple(
                vm => vm.LayoutBackwardKeyCodes, vm => vm.PreferencesModel.SwitchSettings.LayoutBackwardKeys));

            base.EnableChangeTracking();
        }

        protected override Task<PreferencesModel> OnSaveAsync()
        {
            this.PreferencesModel.InstantSwitching = this.InstantSwitching;
            this.PreferencesModel.SwitchLayout = this.SwitchLayout;
            this.PreferencesModel.Startup = this.Startup;
            this.PreferencesModel.ShowUninstalledLayoutsMessage = this.ShowUninstalledLayoutsMessage;
            this.PreferencesModel.ShowConverter = this.ShowConverter;

            var switchSettings = this.PreferencesModel.SwitchSettings;

            switchSettings.ForwardModifierKeys = new(this.forwardModifierKeys);
            switchSettings.BackwardModifierKeys = new(this.backwardModifierKeys);
            switchSettings.PressCount = this.PressCount;
            switchSettings.WaitMilliseconds = this.WaitMilliseconds;

            switchSettings.LayoutForwardKeys.Clear();
            switchSettings.LayoutForwardKeys.AddRange(this.LayoutForwardKeyCodes);

            switchSettings.LayoutBackwardKeys.Clear();
            switchSettings.LayoutBackwardKeys.AddRange(this.LayoutBackwardKeyCodes);

            return Task.FromResult(this.PreferencesModel);
        }

        protected override void CopyProperties()
        {
            this.InstantSwitching = this.PreferencesModel.InstantSwitching;
            this.SwitchLayout = this.PreferencesModel.SwitchLayout;
            this.Startup = this.PreferencesModel.Startup;
            this.ShowUninstalledLayoutsMessage = this.PreferencesModel.ShowUninstalledLayoutsMessage;
            this.ShowConverter = this.PreferencesModel.ShowConverter;

            var switchSettings = this.PreferencesModel.SwitchSettings;

            if (switchSettings.ForwardModifierKeys.Count > 0)
            {
                this.ForwardModifierKeyFirst = switchSettings.ForwardModifierKeys[0];
            }

            if (switchSettings.ForwardModifierKeys.Count > 1)
            {
                this.ForwardModifierKeySecond = switchSettings.ForwardModifierKeys[1];
            }

            if (switchSettings.ForwardModifierKeys.Count > 2)
            {
                this.ForwardModifierKeyThird = switchSettings.ForwardModifierKeys[2];
            }

            if (switchSettings.BackwardModifierKeys.Count > 0)
            {
                this.BackwardModifierKeyFirst = switchSettings.BackwardModifierKeys[0];
            }

            if (switchSettings.BackwardModifierKeys.Count > 1)
            {
                this.BackwardModifierKeySecond = switchSettings.BackwardModifierKeys[1];
            }

            if (switchSettings.BackwardModifierKeys.Count > 2)
            {
                this.BackwardModifierKeyThird = switchSettings.BackwardModifierKeys[2];
            }

            this.PressCount = switchSettings.PressCount;
            this.WaitMilliseconds = switchSettings.WaitMilliseconds;

            this.layoutForwardKeyCodesSource.Edit(list =>
            {
                list.Clear();
                switchSettings.LayoutForwardKeys.ForEach(list.AddOrUpdate);
            });

            this.layoutBackwardKeyCodesSource.Edit(list =>
            {
                list.Clear();
                switchSettings.LayoutBackwardKeys.ForEach(list.AddOrUpdate);
            });
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
                this.forwardModifierKeysSource
                    .Connect()
                    .ToCollection()
                    .Select(this.ContainsDistinctElements),
                this.backwardModifierKeysSource
                    .Connect()
                    .ToCollection()
                    .Select(this.ContainsDistinctElements),
                (forward, backward) => forward && backward);

            return this.LocalizedValidationRule(modifierKeysAreDifferent, "ModifierKeysAreSame");
        }

        private ValidationHelper InitSwitchMethodsAreDifferentRule()
        {
            var switchMethodsAreDifferent = Observable.CombineLatest(
                this.forwardModifierKeysSource.Connect().ToCollection(),
                this.backwardModifierKeysSource.Connect().ToCollection(),
                (forward, backward) => !new HashSet<ModifierKey>(forward).SetEquals(backward));

            return this.LocalizedValidationRule(switchMethodsAreDifferent, "SwitchMethodsAreSame");
        }

        private ValidationHelper InitLayoutKeysAreNotEmptyRule(SourceCache<KeyCode, KeyCode> keys, string messageKey)
        {
            var layoutKeysAreNotEmptyRule = keys.Connect()
                .ToCollection()
                .Select(k => k.Count != 0);

            return this.LocalizedValidationRule(layoutKeysAreNotEmptyRule, messageKey);
        }

        private ValidationHelper InitLayoutKeysAreDifferentRule()
        {
            var switchMethodsAreDifferent = Observable.CombineLatest(
                this.layoutForwardKeyCodesSource.Connect().ToCollection(),
                this.layoutBackwardKeyCodesSource.Connect().ToCollection(),
                (forward, backward) => !new HashSet<KeyCode>(forward).SetEquals(backward));

            return this.LocalizedValidationRule(switchMethodsAreDifferent, "LayoutKeysAreSame");
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

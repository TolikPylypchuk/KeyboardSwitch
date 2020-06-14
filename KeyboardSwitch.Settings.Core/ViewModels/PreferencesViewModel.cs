using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class PreferencesViewModel : ReactiveForm<PreferencesModel, PreferencesViewModel>
    {
        public PreferencesViewModel(
            PreferencesModel preferencesModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.PreferencesModel = preferencesModel;
            this.CopyProperties();

            this.WhenAnyValue(vm => vm.SwitchMode)
                .Merge(this.Cancel.Select(_ => this.SwitchMode))
                .Select<SwitchMode, ReactiveObject>(mode => mode switch
                {
                    SwitchMode.HotKey => this.HotKeySwitchViewModel,
                    SwitchMode.ModifierKey => this.ModifierKeysSwitchModel,
                    _ => throw new NotSupportedException($"Switch mode {mode} is not supported")
                })
                .ToPropertyEx(this, vm => vm.Content);

            this.EnableChangeTracking();
        }

        public PreferencesModel PreferencesModel { get; }

        [Reactive]
        public HotKeySwitchViewModel HotKeySwitchViewModel { get; private set; } = null!;

        [Reactive]
        public ModifierKeysSwitchViewModel ModifierKeysSwitchModel { get; private set; } = null!;

        [Reactive]
        public SwitchMode SwitchMode { get; set; }

        [Reactive]
        public bool InstantSwitching { get; set; }

        [Reactive]
        public bool SwitchLayout { get; set; }

        [Reactive]
        public bool ShowUninstalledLayoutsMessage { get; set; }

        public ReactiveObject? Content { [ObservableAsProperty] get; }

        protected override PreferencesViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.SwitchMode, vm => vm.PreferencesModel.SwitchMode);
            this.TrackChanges(vm => vm.InstantSwitching, vm => vm.PreferencesModel.InstantSwitching);
            this.TrackChanges(vm => vm.SwitchLayout, vm => vm.PreferencesModel.SwitchLayout);
            this.TrackChanges(
                vm => vm.ShowUninstalledLayoutsMessage, vm => vm.PreferencesModel.ShowUninstalledLayoutsMessage);

            this.TrackChanges(this.WhenAnyObservable(vm => vm.HotKeySwitchViewModel.FormChanged));
            this.TrackChanges(this.WhenAnyObservable(vm => vm.ModifierKeysSwitchModel.FormChanged));

            this.TrackValidation(this.WhenAnyObservable(vm => vm.HotKeySwitchViewModel.Valid));
            this.TrackValidation(this.WhenAnyObservable(vm => vm.ModifierKeysSwitchModel.Valid));

            base.EnableChangeTracking();
        }

        protected override async Task<PreferencesModel> OnSaveAsync()
        {
            this.PreferencesModel.SwitchMode = this.SwitchMode;
            this.PreferencesModel.InstantSwitching = this.InstantSwitching;
            this.PreferencesModel.SwitchLayout = this.SwitchLayout;
            this.PreferencesModel.ShowUninstalledLayoutsMessage = this.ShowUninstalledLayoutsMessage;

            await this.HotKeySwitchViewModel.Save.Execute();
            await this.ModifierKeysSwitchModel.Save.Execute();

            return this.PreferencesModel;
        }

        protected override void CopyProperties()
        {
            this.HotKeySwitchViewModel = new HotKeySwitchViewModel(this.PreferencesModel.HotKeySwitchSettings);
            this.ModifierKeysSwitchModel = new ModifierKeysSwitchViewModel(
                this.PreferencesModel.ModifierKeysSwitchSettings);

            this.SwitchMode = this.PreferencesModel.SwitchMode;
            this.InstantSwitching = this.PreferencesModel.InstantSwitching;
            this.SwitchLayout = this.PreferencesModel.SwitchLayout;
            this.ShowUninstalledLayoutsMessage = this.PreferencesModel.ShowUninstalledLayoutsMessage;
        }
    }
}

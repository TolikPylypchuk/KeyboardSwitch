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
    public sealed class PreferencesViewModel : FormBase<PreferencesModel, PreferencesViewModel>
    {
        public PreferencesViewModel(
            PreferencesModel preferencesModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.PreferencesModel = preferencesModel;

            this.WhenAnyValue(vm => vm.SwitchMode)
                .Select<SwitchMode, ReactiveObject>(mode => mode switch
                {
                    SwitchMode.HotKey => this.HotKeySwitchViewModel,
                    SwitchMode.ModifierKey => this.ModifierKeysSwitchModel,
                    _ => throw new NotSupportedException($"Switch mode {mode} is not supported")
                })
                .ToPropertyEx(this, vm => vm.Content);

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public PreferencesModel PreferencesModel { get; }

        [Reactive]
        public HotKeySwitchViewModel HotKeySwitchViewModel { get; private set; } = null!;

        [Reactive]
        public ModifierKeysSwitchViewModel ModifierKeysSwitchModel { get; private set; } = null!;

        [Reactive]
        public SwitchMode SwitchMode { get; set; }

        public ReactiveObject? Content { [ObservableAsProperty] get; }

        protected override PreferencesViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.SwitchMode, vm => vm.PreferencesModel.SwitchMode);
            this.TrackChanges(
                this.WhenAnyValue(vm => vm.HotKeySwitchViewModel).Select(vm => vm.FormChanged).Switch());
            this.TrackChanges(
                this.WhenAnyValue(vm => vm.ModifierKeysSwitchModel).Select(vm => vm.FormChanged).Switch());

            base.EnableChangeTracking();
        }

        protected override async Task<PreferencesModel> OnSaveAsync()
        {
            this.PreferencesModel.SwitchMode = this.SwitchMode;

            await this.HotKeySwitchViewModel.Save.Execute();
            await this.ModifierKeysSwitchModel.Save.Execute();

            return this.PreferencesModel;
        }

        protected override void CopyProperties()
        {
            this.SwitchMode = this.PreferencesModel.SwitchMode;
            this.HotKeySwitchViewModel = new HotKeySwitchViewModel(this.PreferencesModel.HotKeySwitchSettings);
            this.ModifierKeysSwitchModel = new ModifierKeysSwitchViewModel(
                this.PreferencesModel.ModifierKeysSwitchSettings);
        }
    }
}

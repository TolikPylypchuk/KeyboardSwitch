using System.Reactive.Concurrency;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;

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

            this.ValidationRule(vm => vm.PressCount, count => count > 0 && count <= 10);
            this.ValidationRule(vm => vm.WaitMilliseconds, wait => wait >= 100 && wait <= 1000);

            var switchMethodsAreDifferent = this.WhenAnyValue(
                vm => vm.ForwardModifierKeys,
                vm => vm.BackwardModifierKeys,
                (forward, backward) => forward != backward);

            this.SwitchMethodsAreDifferentRule = this.LocalizedValidationRule(
                switchMethodsAreDifferent, "SwitchMethodsAreSame");

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
        public ModifierKeys ForwardModifierKeys { get; set; }

        [Reactive]
        public ModifierKeys BackwardModifierKeys { get; set; }

        [Reactive]
        public int PressCount { get; set; }

        [Reactive]
        public int WaitMilliseconds { get; set; }

        public ValidationHelper SwitchMethodsAreDifferentRule { get; }

        protected override PreferencesViewModel Self => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.InstantSwitching, vm => vm.PreferencesModel.InstantSwitching);
            this.TrackChanges(vm => vm.SwitchLayout, vm => vm.PreferencesModel.SwitchLayout);
            this.TrackChanges(vm => vm.Startup, vm => vm.PreferencesModel.Startup);

            this.TrackChanges(
                vm => vm.ShowUninstalledLayoutsMessage, vm => vm.PreferencesModel.ShowUninstalledLayoutsMessage);

            this.TrackChanges(
                vm => vm.ForwardModifierKeys, vm => vm.PreferencesModel.SwitchSettings.ForwardModifierKeys);

            this.TrackChanges(
                vm => vm.BackwardModifierKeys, vm => vm.PreferencesModel.SwitchSettings.BackwardModifierKeys);

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

            this.PreferencesModel.SwitchSettings.ForwardModifierKeys = this.ForwardModifierKeys;
            this.PreferencesModel.SwitchSettings.BackwardModifierKeys = this.BackwardModifierKeys;
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

            this.ForwardModifierKeys = this.PreferencesModel.SwitchSettings.ForwardModifierKeys;
            this.BackwardModifierKeys = this.PreferencesModel.SwitchSettings.BackwardModifierKeys;
            this.PressCount = this.PreferencesModel.SwitchSettings.PressCount;
            this.WaitMilliseconds = this.PreferencesModel.SwitchSettings.WaitMilliseconds;
        }
    }
}

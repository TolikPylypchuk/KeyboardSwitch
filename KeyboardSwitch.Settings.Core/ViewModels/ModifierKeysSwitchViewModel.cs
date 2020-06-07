using System.Reactive.Concurrency;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Settings;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class ModifierKeysSwitchViewModel : ReactiveForm<ModifierKeysSwitchSettings, ModifierKeysSwitchViewModel>
    {
        public ModifierKeysSwitchViewModel(
            ModifierKeysSwitchSettings settings,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.ModifierKeysSwitchSettings = settings;
            this.CopyProperties();

            this.ValidationRule(vm => vm.PressCount, count => count > 0 && count <= 10);
            this.ValidationRule(vm => vm.WaitMilliseconds, wait => wait >= 100 && wait <= 1000);

            var switchMethodsAreDifferent = this.WhenAnyValue(
                vm => vm.ForwardModifierKeys,
                vm => vm.BackwardModifierKeys,
                (forward, backward) => forward != backward);

            this.SwitchMethodsAreDifferentRule = this.ValidationRule(switchMethodsAreDifferent, "SwitchMethodsAreSame");

            this.EnableChangeTracking();
        }

        public ModifierKeysSwitchSettings ModifierKeysSwitchSettings { get; }

        [Reactive]
        public ModifierKeys ForwardModifierKeys { get; set; }

        [Reactive]
        public ModifierKeys BackwardModifierKeys { get; set; }

        [Reactive]
        public int PressCount { get; set; }

        [Reactive]
        public int WaitMilliseconds { get; set; }

        public ValidationHelper SwitchMethodsAreDifferentRule { get; }

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.ForwardModifierKeys, vm => vm.ModifierKeysSwitchSettings.ForwardModifierKeys);
            this.TrackChanges(vm => vm.BackwardModifierKeys, vm => vm.ModifierKeysSwitchSettings.BackwardModifierKeys);
            this.TrackChanges(vm => vm.PressCount, vm => vm.ModifierKeysSwitchSettings.PressCount);
            this.TrackChanges(vm => vm.WaitMilliseconds, vm => vm.ModifierKeysSwitchSettings.WaitMilliseconds);

            base.EnableChangeTracking();
        }

        protected override ModifierKeysSwitchViewModel Self
            => this;

        protected override Task<ModifierKeysSwitchSettings> OnSaveAsync()
        {
            this.ModifierKeysSwitchSettings.ForwardModifierKeys = this.ForwardModifierKeys;
            this.ModifierKeysSwitchSettings.BackwardModifierKeys = this.BackwardModifierKeys;
            this.ModifierKeysSwitchSettings.PressCount = this.PressCount;
            this.ModifierKeysSwitchSettings.WaitMilliseconds = this.WaitMilliseconds;

            return Task.FromResult(this.ModifierKeysSwitchSettings);
        }

        protected override void CopyProperties()
        {
            this.ForwardModifierKeys = this.ModifierKeysSwitchSettings.ForwardModifierKeys;
            this.BackwardModifierKeys = this.ModifierKeysSwitchSettings.BackwardModifierKeys;
            this.PressCount = this.ModifierKeysSwitchSettings.PressCount;
            this.WaitMilliseconds = this.ModifierKeysSwitchSettings.WaitMilliseconds;
        }
    }
}
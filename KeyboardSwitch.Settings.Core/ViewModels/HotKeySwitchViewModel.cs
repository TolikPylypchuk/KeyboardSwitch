using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Helpers;

using static KeyboardSwitch.Common.Constants;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class HotKeySwitchViewModel : ReactiveForm<HotKeySwitchSettings, HotKeySwitchViewModel>
    {
        private static readonly IImmutableSet<char> AllowedCharacters = ImmutableHashSet.CreateRange(
            "QWERTYUIOP[]\\ASDFGHJKL;'ZXCVBNM,./1234567890-=").Add(MissingCharacter);

        public HotKeySwitchViewModel(
            HotKeySwitchSettings settings,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.HotKeySwitchSettings = settings;
            this.CopyProperties();

            var forwardIsPresent = this.WhenAnyValue(vm => vm.Forward.Character)
                .Select(ch => ch != MissingCharacter);

            var forwardIsValid = this.WhenAnyValue(vm => vm.Forward.Character)
                .Select(AllowedCharacters.Contains);

            this.ForwardIsRequiredRule = this.ValidationRule(forwardIsPresent, "ForwardRequired");
            this.ForwardIsValidRule = this.ValidationRule(forwardIsValid, "ForwardInvalid");

            var backwardIsPresent = this.WhenAnyValue(vm => vm.Backward.Character)
                .Select(ch => ch != MissingCharacter);

            var backwardIsValid = this.WhenAnyValue(vm => vm.Backward.Character)
                .Select(AllowedCharacters.Contains);

            this.BackwardIsRequiredRule = this.ValidationRule(backwardIsPresent, "BackwardRequired");
            this.BackwardIsValidRule = this.ValidationRule(backwardIsValid, "BackwardInvalid");

            var switchMethodsAreDifferent = 
                this.WhenAnyValue(vm => vm.Forward.Character, vm => vm.Backward.Character, (f, b) => f != b);

            this.SwitchMethodsAreDifferentRule = this.ValidationRule(switchMethodsAreDifferent, "SwitchMethodsAreSame");

            this.EnableChangeTracking();
        }

        public HotKeySwitchSettings HotKeySwitchSettings { get; }

        [Reactive]
        public ModifierKeys ModifierKeys { get; set; }

        [Reactive]
        public CharacterViewModel Forward { get; set; } = null!;

        [Reactive]
        public CharacterViewModel Backward { get; set; } = null!;

        public ValidationHelper ForwardIsRequiredRule { get; }
        public ValidationHelper ForwardIsValidRule { get; }

        public ValidationHelper BackwardIsRequiredRule { get; }
        public ValidationHelper BackwardIsValidRule { get; }

        public ValidationHelper SwitchMethodsAreDifferentRule { get; }

        protected override HotKeySwitchViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.ModifierKeys, vm => vm.HotKeySwitchSettings.ModifierKeys);
            this.TrackChanges(this.WhenAnyValue(vm => vm.Forward).Select(vm => vm.FormChanged).Switch());
            this.TrackChanges(this.WhenAnyValue(vm => vm.Backward).Select(vm => vm.FormChanged).Switch());

            base.EnableChangeTracking();
        }

        protected override async Task<HotKeySwitchSettings> OnSaveAsync()
        {
            await this.Forward.Save.Execute();
            await this.Backward.Save.Execute();

            this.HotKeySwitchSettings.ModifierKeys = this.ModifierKeys;
            this.HotKeySwitchSettings.Forward = this.Forward.CharacterModel.Character;
            this.HotKeySwitchSettings.Backward = this.Backward.CharacterModel.Character;

            return this.HotKeySwitchSettings;
        }

        protected override void CopyProperties()
        {
            this.ModifierKeys = this.HotKeySwitchSettings.ModifierKeys;
            this.Forward = new CharacterViewModel(
                new CharacterModel { Character = this.HotKeySwitchSettings.Forward });
            this.Backward = new CharacterViewModel(
                new CharacterModel { Character = this.HotKeySwitchSettings.Backward });
        }
    }
}

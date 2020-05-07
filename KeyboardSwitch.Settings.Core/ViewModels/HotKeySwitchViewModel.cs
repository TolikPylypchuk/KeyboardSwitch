using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Settings;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using static KeyboardSwitch.Common.Constants;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class HotKeySwitchViewModel : FormBase<HotKeySwitchSettings, HotKeySwitchViewModel>
    {
        private static readonly IImmutableSet<char> AllowedCharacters = ImmutableHashSet.CreateRange(
            "QWERTYUIOP[]\\ASDFGHJKL;'ZXCVBNM,./1234567890-=");

        public HotKeySwitchViewModel(
            HotKeySwitchSettings settings,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.HotKeySwitchSettings = settings;
            this.CopyProperties();

            this.ValidationRule(vm => vm.Forward, vm => vm.Character != MissingCharacter, ValidationType.Required);
            this.ValidationRule(vm => vm.Forward, vm => AllowedCharacters.Contains(vm.Character));

            this.ValidationRule(vm => vm.Backward, vm => vm.Character != MissingCharacter, ValidationType.Required);
            this.ValidationRule(vm => vm.Backward, vm => AllowedCharacters.Contains(vm.Character));

            this.EnableChangeTracking();
        }

        public HotKeySwitchSettings HotKeySwitchSettings { get; }

        [Reactive]
        public ModifierKeys ModifierKeys { get; set; }

        [Reactive]
        public CharacterViewModel Forward { get; set; } = null!;

        [Reactive]
        public CharacterViewModel Backward { get; set; } = null!;

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

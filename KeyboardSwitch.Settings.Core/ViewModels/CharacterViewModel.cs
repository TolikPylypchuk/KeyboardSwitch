using System.Reactive.Concurrency;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI.Fody.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class CharacterViewModel : FormBase<CharacterModel, CharacterViewModel>
    {
        public CharacterViewModel(
            CharacterModel characterModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.CharacterModel = characterModel;

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public CharacterModel CharacterModel { get; }

        [Reactive]
        public char Character { get; set; }

        [Reactive]
        public int Index { get; set; }

        protected override CharacterViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.Character, vm => vm.CharacterModel.Character);
            this.TrackChanges(vm => vm.Index, vm => vm.CharacterModel.Index);

            base.EnableChangeTracking();
        }

        protected override Task<CharacterModel> OnSaveAsync()
        {
            this.CharacterModel.Character = this.Character;
            this.CharacterModel.Index = this.Index;

            return Task.FromResult(this.CharacterModel);
        }

        protected override void CopyProperties()
        {
            this.Character = this.CharacterModel.Character;
            this.Index = this.CharacterModel.Index;
        }
    }
}

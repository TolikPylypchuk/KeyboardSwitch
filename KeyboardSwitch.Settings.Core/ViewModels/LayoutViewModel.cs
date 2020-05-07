using System;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI.Fody.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class LayoutViewModel : FormBase<LayoutModel, LayoutViewModel>
    {
        private readonly SourceCache<CharacterModel, int> charactersSource =
            new SourceCache<CharacterModel, int>(ch => ch.Index);

        private readonly ReadOnlyObservableCollection<CharacterViewModel> characters;

        public LayoutViewModel(
            LayoutModel layoutModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.LayoutModel = layoutModel;

            this.charactersSource.Connect()
                .Transform(ch => new CharacterViewModel(ch))
                .Sort(SortExpressionComparer<CharacterViewModel>.Ascending(vm => vm.Index))
                .AutoRefresh(vm => vm.Index)
                .Bind(out this.characters)
                .Subscribe();

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public LayoutModel LayoutModel { get; }

        [Reactive]
        public string LanguageName { get; set; } = String.Empty;

        [Reactive]
        public string KeyboardName { get; set; } = String.Empty;

        [Reactive]
        public int Id { get; set; }

        [Reactive]
        public int Index { get; set; }

        public ReadOnlyObservableCollection<CharacterViewModel> Characters
            => this.characters;

        protected override LayoutViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.LanguageName, vm => vm.LayoutModel.LanguageName);
            this.TrackChanges(vm => vm.KeyboardName, vm => vm.LayoutModel.KeyboardName);
            this.TrackChanges(vm => vm.Id, vm => vm.LayoutModel.Id);
            this.TrackChanges(vm => vm.Index, vm => vm.LayoutModel.Index);
            this.TrackChanges(this.IsCollectionChanged(vm => vm.Characters, vm => vm.LayoutModel.Chars));

            base.EnableChangeTracking();
        }

        protected override async Task<LayoutModel> OnSaveAsync()
        {
            foreach (var ch in this.Characters)
            {
                await ch.Save.Execute();
            }

            this.LayoutModel.LanguageName = this.LanguageName;
            this.LayoutModel.KeyboardName = this.KeyboardName;
            this.LayoutModel.Id = this.Id;
            this.LayoutModel.Index = this.Index;

            this.LayoutModel.Chars.Clear();
            this.LayoutModel.Chars.AddRange(this.charactersSource.Items);

            return this.LayoutModel;
        }

        protected override void CopyProperties()
        {
            this.LanguageName = this.LayoutModel.LanguageName;
            this.KeyboardName = this.LayoutModel.KeyboardName;
            this.Id = this.LayoutModel.Id;
            this.Index = this.LayoutModel.Index;

            this.charactersSource.Edit(list =>
            {
                list.Clear();
                list.AddOrUpdate(this.LayoutModel.Chars);
            });
        }
    }
}

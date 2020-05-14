using System;
using System.Reactive.Concurrency;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI.Fody.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class LayoutViewModel : FormBase<LayoutModel, LayoutViewModel>
    {
        public LayoutViewModel(
            LayoutModel layoutModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.LayoutModel = layoutModel;

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

        [Reactive]
        public string Chars { get; set; } = String.Empty;

        protected override LayoutViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.LanguageName, vm => vm.LayoutModel.LanguageName);
            this.TrackChanges(vm => vm.KeyboardName, vm => vm.LayoutModel.KeyboardName);
            this.TrackChanges(vm => vm.Id, vm => vm.LayoutModel.Id);
            this.TrackChanges(vm => vm.Index, vm => vm.LayoutModel.Index);
            this.TrackChanges(vm => vm.Chars, vm => vm.LayoutModel.Chars);

            base.EnableChangeTracking();
        }

        protected override Task<LayoutModel> OnSaveAsync()
        {
            this.LayoutModel.LanguageName = this.LanguageName;
            this.LayoutModel.KeyboardName = this.KeyboardName;
            this.LayoutModel.Id = this.Id;
            this.LayoutModel.Index = this.Index;
            this.LayoutModel.Chars = this.Chars;

            return Task.FromResult(this.LayoutModel);
        }

        protected override void CopyProperties()
        {
            this.LanguageName = this.LayoutModel.LanguageName;
            this.KeyboardName = this.LayoutModel.KeyboardName;
            this.Id = this.LayoutModel.Id;
            this.Index = this.LayoutModel.Index;
            this.Chars = this.LayoutModel.Chars;
        }
    }
}

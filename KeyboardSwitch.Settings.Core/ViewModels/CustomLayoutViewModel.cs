using System;
using System.Reactive.Concurrency;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class CustomLayoutViewModel : ReactiveForm<CustomLayoutModel, CustomLayoutViewModel>
    {
        public CustomLayoutViewModel(
            CustomLayoutModel customLayoutModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.CustomLayoutModel = customLayoutModel;

            this.NameRule = this.ValidationRule(vm => vm.Name, name => !String.IsNullOrWhiteSpace(name), "NameEmpty");

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public CustomLayoutModel CustomLayoutModel { get; }

        [Reactive]
        public string Name { get; set; } = String.Empty;

        [Reactive]
        public string Chars { get; set; } = String.Empty;

        public ValidationHelper NameRule { get; }

        protected override CustomLayoutViewModel Self
            => this;

        protected override void EnableChangeTracking()
        {
            this.TrackChanges(vm => vm.Name, vm => vm.CustomLayoutModel.Name);
            this.TrackChanges(vm => vm.Chars, vm => vm.CustomLayoutModel.Chars);

            base.EnableChangeTracking();
        }

        protected override Task<CustomLayoutModel> OnSaveAsync()
        {
            this.CustomLayoutModel.Name = this.Name;
            this.CustomLayoutModel.Chars = this.Chars;

            return Task.FromResult(this.CustomLayoutModel);
        }

        protected override void CopyProperties()
        {
            this.Name = this.CustomLayoutModel.Name;
            this.Chars = this.CustomLayoutModel.Chars;
        }
    }
}

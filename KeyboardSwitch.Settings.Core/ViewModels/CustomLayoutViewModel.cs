using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using static KeyboardSwitch.Common.Constants;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class CustomLayoutViewModel : ReactiveForm<CustomLayoutModel, CustomLayoutViewModel>
    {
        public CustomLayoutViewModel(
            CustomLayoutModel customLayoutModel,
            bool isNew,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.CustomLayoutModel = customLayoutModel;
            this.IsNew = isNew;

            this.ValidationRule(
                vm => vm.Name, name => !String.IsNullOrWhiteSpace(name), this.ResourceManager.GetString("NameEmpty"));

            this.ValidationRule(
                vm => vm.Chars,
                chars => chars.Distinct().Count(ch => ch != MissingCharacter) ==
                    chars.Count(ch => ch != MissingCharacter),
                chars => String.Format(
                    this.ResourceManager.GetString("CharsDuplicatedFormat") ?? String.Empty,
                    chars
                        .Where(ch => ch != MissingCharacter)
                        .GroupBy(ch => ch)
                        .Where(chs => chs.Count() > 1)
                        .Select(chs => chs.Key.ToString())
                        .Aggregate((acc, ch) => $"{acc}, {ch}")));

            this.CopyProperties();
            this.CanAlwaysDelete();
            this.EnableChangeTracking();
        }

        public CustomLayoutModel CustomLayoutModel { get; }

        [Reactive]
        public string Name { get; set; } = String.Empty;

        [Reactive]
        public string Chars { get; set; } = String.Empty;

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
            this.IsNew = false;

            return Task.FromResult(this.CustomLayoutModel);
        }

        protected override Task<CustomLayoutModel?> OnDeleteAsync()
        {
            this.IsDeleted = true;
            return Task.FromResult<CustomLayoutModel?>(this.CustomLayoutModel);
        }

        protected override void CopyProperties()
        {
            this.Name = this.CustomLayoutModel.Name;
            this.Chars = this.CustomLayoutModel.Chars;
        }
    }
}

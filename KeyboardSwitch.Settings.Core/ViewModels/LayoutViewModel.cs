using System;
using System.Globalization;
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
    public sealed class LayoutViewModel : ReactiveForm<LayoutModel, LayoutViewModel>
    {
        public LayoutViewModel(
            LayoutModel layoutModel,
            ResourceManager? resourceManager = null,
            IScheduler? scheduler = null)
            : base(resourceManager, scheduler)
        {
            this.LayoutModel = layoutModel;

            this.ValidationRule(
                vm => vm.Chars,
                chars => chars.Distinct().Count(ch => ch != MissingCharacter) ==
                    chars.Count(ch => ch != MissingCharacter),
                chars => String.Format(
                    CultureInfo.InvariantCulture,
                    this.ResourceManager.GetString("CharsDuplicatedFormat") ?? String.Empty,
                    chars
                        .Where(ch => ch != MissingCharacter)
                        .GroupBy(ch => ch)
                        .Where(chs => chs.Count() > 1)
                        .Select(chs => chs.Key.ToString())
                        .Aggregate((acc, ch) => $"{acc}, {ch}")));

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
            this.LayoutModel.IsNew = this.IsNew = false;

            return Task.FromResult(this.LayoutModel);
        }

        protected override void CopyProperties()
        {
            this.LanguageName = this.LayoutModel.LanguageName;
            this.KeyboardName = this.LayoutModel.KeyboardName;
            this.Id = this.LayoutModel.Id;
            this.Index = this.LayoutModel.Index;
            this.IsNew = this.LayoutModel.IsNew;
            this.Chars = this.LayoutModel.Chars;
        }
    }
}

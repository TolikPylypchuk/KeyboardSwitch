using System.Reactive;
using System.Resources;
using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

using Splat;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class ConverterViewModel : ReactiveValidationObject<ConverterViewModel>
    {
        public ConverterViewModel(ConverterModel converterModel, ResourceManager? resourceManager = null)
        {
            this.ConverterModel = converterModel;
            resourceManager ??= Locator.Current.GetService<ResourceManager>();

            this.LayoutsAreDifferentRule = this.ValidationRule(
                vm => vm.WhenAnyValue(v => v.SourceLayout, v => v.TargetLayout, (s, t) => s != t),
                (vm, state) => resourceManager.GetString("CustomLayoutsAreSame"));

            this.Convert = ReactiveCommand.CreateFromTask<string, string>(this.ConvertAsync);
            this.ReverseLayouts = ReactiveCommand.Create(this.OnReverseLayouts);
        }

        public ConverterModel ConverterModel { get; }

        [Reactive]
        public CustomLayoutModel? SourceLayout { get; set; }

        [Reactive]
        public CustomLayoutModel? TargetLayout { get; set; }

        public ValidationHelper LayoutsAreDifferentRule { get; }

        public ReactiveCommand<string, string> Convert { get; }
        public ReactiveCommand<Unit, Unit> ReverseLayouts { get; }

        private Task<string> ConvertAsync(string text)
        {
            return Task.FromResult(text);
        }

        private void OnReverseLayouts()
            => (this.SourceLayout, this.TargetLayout) = (this.TargetLayout, this.SourceLayout);
    }
}

using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Resources;
using System.Threading.Tasks;

using DynamicData;
using DynamicData.Binding;

using KeyboardSwitch.Core;
using KeyboardSwitch.Core.Services.Layout;
using KeyboardSwitch.Core.Services.Settings;
using KeyboardSwitch.Core.Services.Switching;
using KeyboardSwitch.Core.Services.Text;
using KeyboardSwitch.Settings.Core.Models;
using KeyboardSwitch.Settings.Core.Services;

using Microsoft.Extensions.Logging;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

using static KeyboardSwitch.Settings.Core.ServiceUtil;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class ConverterViewModel : ReactiveValidationObject, ITextService
    {
        private readonly ISwitchService switchService;

        private readonly ReadOnlyObservableCollection<CustomLayoutModel> layouts;

        public ConverterViewModel(
            ConverterModel converterModel,
            ILayoutService? layoutService = null,
            IAppSettingsService? settingsService = null,
            ResourceManager? resourceManager = null)
        {
            this.ConverterModel = converterModel;

            this.switchService = this.CreateSwitchService(layoutService, settingsService);

            resourceManager ??= GetDefaultService<ResourceManager>();

            this.ConverterModel.Layouts
                .ToObservableChangeSet()
                .Sort(SortExpressionComparer<CustomLayoutModel>.Ascending(layout => layout.Id))
                .Bind(out this.layouts)
                .Subscribe();

            this.LayoutsAreDifferentRule = this.ValidationRule(
                this.WhenAnyValue(v => v.SourceLayout, v => v.TargetLayout, (s, t) => s == null || t == null || s != t),
                resourceManager.GetString("CustomLayoutsAreSame") ?? String.Empty);

            var canConvert = this.WhenAnyValue(
                vm => vm.SourceLayout, vm => vm.TargetLayout, (s, t) => s != null && t != null)
                .CombineLatest(this.IsValid(), (a, b) => a && b);

            this.Convert = ReactiveCommand.CreateFromTask(this.ConvertAsync, canConvert);
            this.SwapLayouts = ReactiveCommand.Create(this.OnSwapLayouts);
            this.Clear = ReactiveCommand.Create(this.OnClear);
        }

        public ConverterModel ConverterModel { get; }

        [Reactive]
        public string SourceText { get; set; } = String.Empty;

        [Reactive]
        public string TargetText { get; set; } = String.Empty;

        public ReadOnlyObservableCollection<CustomLayoutModel> Layouts => this.layouts;

        [Reactive]
        public CustomLayoutModel? SourceLayout { get; set; }

        [Reactive]
        public CustomLayoutModel? TargetLayout { get; set; }

        public ValidationHelper LayoutsAreDifferentRule { get; }

        public ReactiveCommand<Unit, Unit> Convert { get; }
        public ReactiveCommand<Unit, Unit> SwapLayouts { get; }
        public ReactiveCommand<Unit, Unit> Clear { get; }

        private Task ConvertAsync() =>
            this.switchService.SwitchTextAsync(SwitchDirection.Forward);

        private void OnSwapLayouts()
        {
            (this.SourceLayout, this.TargetLayout) = (this.TargetLayout, this.SourceLayout);
            (this.SourceText, this.TargetText) = (this.TargetText, this.SourceText);
        }

        private void OnClear() =>
            this.SourceText = this.TargetText = String.Empty;

        private ISwitchService CreateSwitchService(
            ILayoutService? layoutService = null,
            IAppSettingsService? settingsService = null)
        {
            var sourceLayout = this.WhenAnyValue(vm => vm.SourceLayout).WhereNotNull();
            var targetLayout = this.WhenAnyValue(vm => vm.TargetLayout).WhereNotNull();

            return new SwitchService(
                this,
                layoutService ?? new ConverterLayoutService(sourceLayout, targetLayout),
                settingsService ?? new ConverterAppSettingsService(sourceLayout, targetLayout),
                GetDefaultService<ILogger<SwitchService>>());
        }

        Task<string?> ITextService.GetTextAsync() =>
            Task.FromResult<string?>(this.SourceText);

        Task ITextService.SetTextAsync(string text)
        {
            this.TargetText = text;
            return Task.CompletedTask;
        }
    }
}

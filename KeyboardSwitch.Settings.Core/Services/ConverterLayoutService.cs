using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Services;
using KeyboardSwitch.Settings.Core.Models;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using static KeyboardSwitch.Settings.Core.Constants;

namespace KeyboardSwitch.Settings.Core.Services
{
    public class ConverterLayoutService : ReactiveObject, ILayoutService
    {
        public ConverterLayoutService(
            IObservable<CustomLayoutModel> sourceLayout,
            IObservable<CustomLayoutModel> targetLayout)
        {
            sourceLayout
                .Select(layout => this.CreateFakeKeyboardLayout(layout, SourceLayoutId))
                .ToPropertyEx(this, vm => vm.SourceLayout);

            targetLayout
                .Select(layout => this.CreateFakeKeyboardLayout(layout, TargetLayoutId))
                .ToPropertyEx(this, vm => vm.TargetLayout);
        }

        private KeyboardLayout SourceLayout { [ObservableAsProperty]get; } = null!;
        private KeyboardLayout TargetLayout { [ObservableAsProperty]get; } = null!;

        public KeyboardLayout GetCurrentKeyboardLayout()
            => this.SourceLayout;

        public List<KeyboardLayout> GetKeyboardLayouts()
            => new List<KeyboardLayout> { this.SourceLayout, this.TargetLayout };

        public void SwitchCurrentLayout(SwitchDirection direction)
        { }

        private KeyboardLayout CreateFakeKeyboardLayout(CustomLayoutModel layout, int index)
            => new KeyboardLayout(index, CultureInfo.InvariantCulture, layout.Name);
    }
}

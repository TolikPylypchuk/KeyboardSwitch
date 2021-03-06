using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;

using KeyboardSwitch.Core;
using KeyboardSwitch.Core.Keyboard;
using KeyboardSwitch.Core.Services.Layout;
using KeyboardSwitch.Core.Settings;
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

        public bool SwitchLayoutsViaKeyboardSimulation => false;

        private KeyboardLayout SourceLayout { [ObservableAsProperty] get; } = null!;
        private KeyboardLayout TargetLayout { [ObservableAsProperty] get; } = null!;

        public KeyboardLayout GetCurrentKeyboardLayout() =>
            this.SourceLayout;

        public List<KeyboardLayout> GetKeyboardLayouts() =>
            new() { this.SourceLayout, this.TargetLayout };

        public void SwitchCurrentLayout(SwitchDirection direction, SwitchSettings settings)
        { }

        private KeyboardLayout CreateFakeKeyboardLayout(CustomLayoutModel layout, string index) =>
            new(index, CultureInfo.InvariantCulture.EnglishName, layout.Name, index);
    }
}

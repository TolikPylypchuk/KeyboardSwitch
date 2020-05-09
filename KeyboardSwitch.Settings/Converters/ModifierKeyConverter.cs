using System;
using System.Collections.Generic;
using System.Linq;

using KeyboardSwitch.Common;
using KeyboardSwitch.Settings.Properties;

using ReactiveUI;

namespace KeyboardSwitch.Settings.Converters
{
    public sealed class ModifierKeyConverter : IBindingTypeConverter
    {
        private readonly Dictionary<ModifierKeys, string> modeToKeys;
        private readonly Dictionary<string, ModifierKeys> keysToMode;

        public ModifierKeyConverter()
        {
            this.modeToKeys = this.Baseline.GetPowerSet()
                .Select(modifiers => modifiers.ToList())
                .Where(modifiers => modifiers.Count > 0)
                .Select(modifiers => new
                {
                    Key = modifiers.Flatten(),
                    Value = modifiers.Select(this.ModifierToString).Aggregate(this.AddModifierStrings)
                })
                .ToDictionary(item => item.Key, item => item.Value);

            this.modeToKeys.Add(ModifierKeys.None, Messages.ModifierKeyNone);

            this.keysToMode = modeToKeys.ToDictionary(e => e.Value, e => e.Key);
        }

        public List<ModifierKeys> Baseline { get; } = new List<ModifierKeys>
        {
            ModifierKeys.Alt,
            ModifierKeys.Ctrl,
            ModifierKeys.Shift,
            ModifierKeys.Win
        };

        public int GetAffinityForObjects(Type fromType, Type toType)
            => fromType == typeof(ModifierKeys) && toType == typeof(string) ||
               fromType == typeof(string) && toType == typeof(ModifierKeys)
                ? 10
                : 0;

        public bool TryConvert(object from, Type toType, object? conversionHint, out object? result)
        {
            switch (from)
            {
                case ModifierKeys keys:
                    result = this.modeToKeys[keys];
                    return true;
                case string str:
                    result = this.keysToMode[str];
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        private string ModifierToString(ModifierKeys modifier)
            => modifier switch
            {
                ModifierKeys.None => Messages.ModifierKeyNone,
                ModifierKeys.Alt => Messages.ModifierKeyAlt,
                ModifierKeys.Ctrl => Messages.ModifierKeyCtrl,
                ModifierKeys.Shift => Messages.ModifierKeyShift,
                ModifierKeys.Win => Messages.ModifierKeyWin,
                _ => String.Empty
            };

        private string AddModifierStrings(string left, string right)
            => left + Messages.ModifierKeysSeparator + right;
    }
}

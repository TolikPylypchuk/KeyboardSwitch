using System;
using System.Collections.Generic;
using System.Linq;

using KeyboardSwitch.Common;
using KeyboardSwitch.Common.Keyboard;
using KeyboardSwitch.Settings.Properties;

namespace KeyboardSwitch.Settings.Converters
{
    public static class Convert
    {
        private static readonly List<ModifierKeys> Baseline = new()
        {
            ModifierKeys.Alt,
            ModifierKeys.Ctrl,
            ModifierKeys.Shift,
            ModifierKeys.Meta
        };

        private static readonly IReadOnlyDictionary<ModifierKeys, string> KeysToString =
            Baseline.GetPowerSet()
                .Select(modifiers => modifiers.ToList())
                .Where(modifiers => modifiers.Count > 0)
                .Select(modifiers => new
                {
                    Key = modifiers.Flatten(),
                    Value = modifiers.Select(ModifierToString).Aggregate(AddModifierStrings)
                })
                .Append(new { Key = ModifierKeys.None, Value = Messages.ModifierKeyNone })
                .ToDictionary(item => item.Key, item => item.Value);

        private static readonly IReadOnlyDictionary<string, ModifierKeys> StringToKeys =
            KeysToString.ToDictionary(e => e.Value, e => e.Key);

        public static string ModifierKeysToString(ModifierKeys modifierKeys) =>
            KeysToString[modifierKeys];

        public static ModifierKeys StringToModifierKeys(string str) =>
            StringToKeys[str];

        private static string ModifierToString(ModifierKeys modifier) =>
            modifier switch
            {
                ModifierKeys.None => Messages.ModifierKeyNone,
                ModifierKeys.Alt => Messages.ModifierKeyAlt,
                ModifierKeys.Ctrl => Messages.ModifierKeyCtrl,
                ModifierKeys.Shift => Messages.ModifierKeyShift,
                ModifierKeys.Meta => Messages.ModifierKeyWin,
                _ => String.Empty
            };

        private static string AddModifierStrings(string left, string right) =>
            left + Messages.ModifierKeysSeparator + right;
    }
}
